namespace Shiko.CourseReviewProvider.Api.Contracts;

public sealed record CourseReviewResponse(
    Guid Id,
    Guid CourseId,
    string UserId,
    string Text,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc
);