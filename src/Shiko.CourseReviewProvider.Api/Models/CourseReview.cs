namespace Shiko.CourseReviewProvider.Api.Models;

public sealed class CourseReview
{
    public Guid Id { get; set; }

    public Guid CourseId { get; set; }

    public string UserId { get; set; } = null!;

    public string Text { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public DateTime? DeletedAtUtc { get; set; }
}