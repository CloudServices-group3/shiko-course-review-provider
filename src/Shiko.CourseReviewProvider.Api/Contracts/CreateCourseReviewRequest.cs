using System.ComponentModel.DataAnnotations;

namespace Shiko.CourseReviewProvider.Api.Contracts;

public sealed record CreateCourseReviewRequest(
    [property: Required]
    [property: StringLength(1500, MinimumLength = 1)]
    string Text
);