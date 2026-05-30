using Shiko.CourseReviewProvider.Api.Clients;

namespace Shiko.CourseReviewProvider.Api.IntegrationTests.TestInfrastructure;

public sealed class FakeCourseRatingClient : ICourseRatingClient
{
    public bool UserHasRating { get; set; } = true;

    public Task<bool> UserHasRatingAsync(
        Guid courseId,
        string accessToken,
        CancellationToken ct = default)
    {
        return Task.FromResult(UserHasRating);
    }
}