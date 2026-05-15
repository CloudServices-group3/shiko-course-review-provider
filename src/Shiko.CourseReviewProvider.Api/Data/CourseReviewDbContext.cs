using Microsoft.EntityFrameworkCore;
using Shiko.CourseReviewProvider.Api.Models;

namespace Shiko.CourseReviewProvider.Api.Data;

public sealed class CourseReviewDbContext(DbContextOptions<CourseReviewDbContext> options)
    : DbContext(options)
{
    public DbSet<CourseReview> CourseReviews => Set<CourseReview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CourseReviewDbContext).Assembly);
    }
}