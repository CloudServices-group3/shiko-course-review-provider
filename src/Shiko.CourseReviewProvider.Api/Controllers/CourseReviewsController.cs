using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shiko.CourseReviewProvider.Api.Contracts;
using Shiko.CourseReviewProvider.Api.Services;

namespace Shiko.CourseReviewProvider.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/course-reviews")]
public sealed class CourseReviewsController(ICourseReviewService courseReviewService) : ControllerBase
{
    [HttpGet("{courseId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<CourseReviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<CourseReviewResponse>>> GetCourseReviews(
        Guid courseId,
        CancellationToken ct)
    {
        var reviews = await courseReviewService.GetCourseReviewsAsync(courseId, ct);

        return Ok(reviews);
    }

    [HttpGet("{courseId:guid}/me")]
    [ProducesResponseType(typeof(CourseReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseReviewResponse>> GetMyCourseReview(
        Guid courseId,
        CancellationToken ct)
    {
        var userId = GetUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "User id claim is missing." });
        }

        var review = await courseReviewService.GetUserCourseReviewAsync(
            courseId,
            userId,
            ct);

        if (review is null)
        {
            return NotFound(new { message = "No review was found for this course and user." });
        }

        return Ok(review);
    }

    [HttpPost("{courseId:guid}")]
    [ProducesResponseType(typeof(CourseReviewResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CourseReviewResponse>> CreateCourseReview(
        Guid courseId,
        CreateCourseReviewRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "User id claim is missing." });
        }

        var accessToken = GetAccessToken();

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Unauthorized(new { message = "Bearer token is missing." });
        }

        try
        {
            var review = await courseReviewService.CreateCourseReviewAsync(
                courseId,
                userId,
                request.Text,
                accessToken,
                ct);

            return CreatedAtAction(
                nameof(GetMyCourseReview),
                new { courseId },
                review);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{courseId:guid}/me")]
    [ProducesResponseType(typeof(CourseReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CourseReviewResponse>> UpdateMyCourseReview(
        Guid courseId,
        UpdateCourseReviewRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "User id claim is missing." });
        }

        var accessToken = GetAccessToken();

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return Unauthorized(new { message = "Bearer token is missing." });
        }

        try
        {
            var review = await courseReviewService.UpdateCourseReviewAsync(
                courseId,
                userId,
                request.Text,
                accessToken,
                ct);

            if (review is null)
            {
                return NotFound(new { message = "No review was found for this course and user." });
            }

            return Ok(review);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{courseId:guid}/me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMyCourseReview(
        Guid courseId,
        CancellationToken ct)
    {
        var userId = GetUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new { message = "User id claim is missing." });
        }

        var deleted = await courseReviewService.SoftDeleteCourseReviewAsync(
            courseId,
            userId,
            ct);

        if (!deleted)
        {
            return NotFound(new { message = "No review was found for this course and user." });
        }

        return NoContent();
    }

    private string? GetUserId()
    {
        return User.FindFirstValue("userId");
    }

    private string? GetAccessToken()
    {
        var authorizationHeader = Request.Headers["Authorization"].ToString();

        const string bearerPrefix = "Bearer ";

        if (!authorizationHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return authorizationHeader[bearerPrefix.Length..].Trim();
    }
}