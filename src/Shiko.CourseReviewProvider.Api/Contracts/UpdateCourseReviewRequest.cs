using System.ComponentModel.DataAnnotations;

namespace Shiko.CourseReviewProvider.Api.Contracts;

public sealed record UpdateCourseReviewRequest(
    [param: Required]
    [param: StringLength(1500, MinimumLength = 1)]
    string Text
);