using System.Net;
using Shiko.CourseReviewProvider.Api.IntegrationTests.TestInfrastructure;
using Xunit;

namespace Shiko.CourseReviewProvider.Api.IntegrationTests.Endpoints;

public sealed class HealthEndpointTests
    : IClassFixture<CourseReviewIntegrationTestFixture>
{
    private readonly CourseReviewIntegrationTestFixture _fixture;

    public HealthEndpointTests(CourseReviewIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await _fixture.Client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}