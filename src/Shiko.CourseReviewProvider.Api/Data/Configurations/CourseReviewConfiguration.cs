using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shiko.CourseReviewProvider.Api.Models;

namespace Shiko.CourseReviewProvider.Api.Data.Configurations;

public sealed class CourseReviewConfiguration : IEntityTypeConfiguration<CourseReview>
{
    public void Configure(EntityTypeBuilder<CourseReview> builder)
    {
        builder.ToTable("CourseReviews", "course_review");

        builder.HasKey(review => review.Id);

        builder.Property(review => review.CourseId)
            .IsRequired();

        builder.Property(review => review.UserId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(review => review.Text)
            .HasMaxLength(1500)
            .IsRequired();

        builder.Property(review => review.IsDeleted)
            .IsRequired();

        builder.Property(review => review.CreatedAtUtc)
            .IsRequired();

        builder.Property(review => review.UpdatedAtUtc)
            .IsRequired();

        builder.Property(review => review.DeletedAtUtc);

        builder.HasIndex(review => new
        {
            review.CourseId,
            review.UserId
        })
            .IsUnique();

        builder.HasIndex(review => review.CourseId);
    }
}git status