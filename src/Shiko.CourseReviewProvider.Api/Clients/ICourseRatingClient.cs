namespace Shiko.CourseReviewProvider.Api.Clients;

public interface ICourseRatingClient
{
    Task<bool> UserHasRatingAsync(
        Guid courseId,
        string accessToken,
        CancellationToken ct = default);
}