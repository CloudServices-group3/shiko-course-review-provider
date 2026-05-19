using Shiko.CourseReviewProvider.Api.Contracts;

namespace Shiko.CourseReviewProvider.Api.Services;

public interface ICourseReviewService
{
    Task<IReadOnlyList<CourseReviewResponse>> GetCourseReviewsAsync(
        Guid courseId,
        CancellationToken ct = default);

    Task<CourseReviewResponse?> GetUserCourseReviewAsync(
        Guid courseId,
        string userId,
        CancellationToken ct = default);

    Task<CourseReviewResponse> CreateCourseReviewAsync(
        Guid courseId,
        string userId,
        string text,
        string accessToken,
        CancellationToken ct = default);

    Task<CourseReviewResponse?> UpdateCourseReviewAsync(
        Guid courseId,
        string userId,
        string text,
        string accessToken,
        CancellationToken ct = default);

    Task<bool> SoftDeleteCourseReviewAsync(
        Guid courseId,
        string userId,
        CancellationToken ct = default);
}