using Microsoft.EntityFrameworkCore;
using Shiko.CourseReviewProvider.Api.Clients;
using Shiko.CourseReviewProvider.Api.Contracts;
using Shiko.CourseReviewProvider.Api.Data;
using Shiko.CourseReviewProvider.Api.Models;

namespace Shiko.CourseReviewProvider.Api.Services;

public sealed class CourseReviewService(
    CourseReviewDbContext dbContext,
    ICourseRatingClient courseRatingClient) : ICourseReviewService
{
    private const int MaxReviewLength = 1500;

    public async Task<IReadOnlyList<CourseReviewResponse>> GetCourseReviewsAsync(
        Guid courseId,
        CancellationToken ct = default)
    {
        return await dbContext.CourseReviews
            .AsNoTracking()
            .Where(review => review.CourseId == courseId && !review.IsDeleted)
            .OrderByDescending(review => review.CreatedAtUtc)
            .Select(review => MapToResponse(review))
            .ToListAsync(ct);
    }

    public async Task<CourseReviewResponse?> GetUserCourseReviewAsync(
        Guid courseId,
        string userId,
        CancellationToken ct = default)
    {
        var review = await dbContext.CourseReviews
            .AsNoTracking()
            .FirstOrDefaultAsync(
                review => review.CourseId == courseId
                    && review.UserId == userId
                    && !review.IsDeleted,
                ct);

        return review is null
            ? null
            : MapToResponse(review);
    }

    public async Task<CourseReviewResponse> CreateCourseReviewAsync(
    Guid courseId,
    string userId,
    string text,
    string accessToken,
    CancellationToken ct = default)
    {
        var cleanedText = CleanReviewText(text);

        var existingReview = await dbContext.CourseReviews
            .FirstOrDefaultAsync(
                review => review.CourseId == courseId && review.UserId == userId,
                ct);

        if (existingReview is not null && !existingReview.IsDeleted)
        {
            throw new InvalidOperationException("A review already exists for this course and user.");
        }

        var hasRating = await courseRatingClient.UserHasRatingAsync(
            courseId,
            accessToken,
            ct);

        if (!hasRating)
        {
            throw new InvalidOperationException("A review cannot be created without an existing course rating.");
        }

        var now = DateTime.UtcNow;

        CourseReview review;

        if (existingReview is not null && existingReview.IsDeleted)
        {
            existingReview.Text = cleanedText;
            existingReview.IsDeleted = false;
            existingReview.CreatedAtUtc = now;
            existingReview.UpdatedAtUtc = now;
            existingReview.DeletedAtUtc = null;

            review = existingReview;
        }
        else
        {
            review = new CourseReview
            {
                Id = Guid.NewGuid(),
                CourseId = courseId,
                UserId = userId,
                Text = cleanedText,
                IsDeleted = false,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
                DeletedAtUtc = null
            };

            dbContext.CourseReviews.Add(review);
        }

        await dbContext.SaveChangesAsync(ct);

        return MapToResponse(review);
    }

    public async Task<CourseReviewResponse?> UpdateCourseReviewAsync(
    Guid courseId,
    string userId,
    string text,
    string accessToken,
    CancellationToken ct = default)
    {
        var cleanedText = CleanReviewText(text);

        var review = await dbContext.CourseReviews
            .FirstOrDefaultAsync(
                review => review.CourseId == courseId
                    && review.UserId == userId
                    && !review.IsDeleted,
                ct);

        if (review is null)
        {
            return null;
        }

        var hasRating = await courseRatingClient.UserHasRatingAsync(
            courseId,
            accessToken,
            ct);

        if (!hasRating)
        {
            throw new InvalidOperationException(
                "A review cannot be updated without an existing course rating.");
        }

        review.Text = cleanedText;
        review.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        return MapToResponse(review);
    }

    public async Task<bool> SoftDeleteCourseReviewAsync(
        Guid courseId,
        string userId,
        CancellationToken ct = default)
    {
        var review = await dbContext.CourseReviews
            .FirstOrDefaultAsync(
                review => review.CourseId == courseId
                    && review.UserId == userId
                    && !review.IsDeleted,
                ct);

        if (review is null)
        {
            return false;
        }

        var now = DateTime.UtcNow;

        review.IsDeleted = true;
        review.UpdatedAtUtc = now;
        review.DeletedAtUtc = now;

        await dbContext.SaveChangesAsync(ct);

        return true;
    }

    private static string CleanReviewText(string text)
    {
        var cleanedText = text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(cleanedText))
        {
            throw new ArgumentException("Review text is required.", nameof(text));
        }

        if (cleanedText.Length > MaxReviewLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(text),
                $"Review text must be {MaxReviewLength} characters or fewer.");
        }

        return cleanedText;
    }

    private static CourseReviewResponse MapToResponse(CourseReview review)
    {
        return new CourseReviewResponse(
            Id: review.Id,
            CourseId: review.CourseId,
            UserId: review.UserId,
            Text: review.Text,
            CreatedAtUtc: review.CreatedAtUtc,
            UpdatedAtUtc: review.UpdatedAtUtc);
    }
}