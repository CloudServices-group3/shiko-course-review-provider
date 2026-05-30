using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Shiko.CourseReviewProvider.Api.Contracts;
using Shiko.CourseReviewProvider.Api.IntegrationTests.TestInfrastructure;
using Xunit;

namespace Shiko.CourseReviewProvider.Api.IntegrationTests.Endpoints;

public sealed class CourseReviewEndpointTests
    : IClassFixture<CourseReviewIntegrationTestFixture>
{
    private readonly CourseReviewIntegrationTestFixture _fixture;

    public CourseReviewEndpointTests(CourseReviewIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetMyCourseReview_WithoutToken_ReturnsUnauthorized()
    {
        await _fixture.ResetDatabaseAsync();

        var courseId = Guid.NewGuid();

        var response = await _fixture.Client.GetAsync(
            $"/api/course-reviews/{courseId}/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateCourseReview_WithUserTokenAndExistingRating_ReturnsCreated()
    {
        await _fixture.ResetDatabaseAsync();

        _fixture.CourseRatingClient.UserHasRating = true;

        var courseId = Guid.NewGuid();

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/course-reviews/{courseId}");

        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            JwtTokenFactory.CreateUserToken("user-1"));

        request.Content = JsonContent.Create(new CreateCourseReviewRequest(
            "This course was useful and easy to follow."));

        var response = await _fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var review = await response.Content
            .ReadFromJsonAsync<CourseReviewResponse>();

        Assert.NotNull(review);
        Assert.Equal(courseId, review.CourseId);
        Assert.Equal("user-1", review.UserId);
        Assert.Equal("This course was useful and easy to follow.", review.Text);
    }

    [Fact]
    public async Task CreateCourseReview_WithUserTokenButNoRating_ReturnsConflict()
    {
        await _fixture.ResetDatabaseAsync();

        _fixture.CourseRatingClient.UserHasRating = false;

        var courseId = Guid.NewGuid();

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/course-reviews/{courseId}");

        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            JwtTokenFactory.CreateUserToken("user-1"));

        request.Content = JsonContent.Create(new CreateCourseReviewRequest(
            "This should not be saved without a rating."));

        var response = await _fixture.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMyCourseReview_WithUserToken_ReturnsNoContentAndReviewIsNotReturned()
    {
        await _fixture.ResetDatabaseAsync();

        _fixture.CourseRatingClient.UserHasRating = true;

        var courseId = Guid.NewGuid();
        var token = JwtTokenFactory.CreateUserToken("user-1");

        using var createRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/course-reviews/{courseId}");

        createRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token);

        createRequest.Content = JsonContent.Create(new CreateCourseReviewRequest(
            "This review should be soft deleted."));

        var createResponse = await _fixture.Client.SendAsync(createRequest);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        using var deleteRequest = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/api/course-reviews/{courseId}/me");

        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token);

        var deleteResponse = await _fixture.Client.SendAsync(deleteRequest);

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        using var getRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/course-reviews/{courseId}/me");

        getRequest.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token);

        var getResponse = await _fixture.Client.SendAsync(getRequest);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}