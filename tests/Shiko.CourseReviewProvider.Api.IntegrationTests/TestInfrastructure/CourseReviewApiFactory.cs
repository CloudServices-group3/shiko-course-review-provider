using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shiko.CourseReviewProvider.Api.Clients;
using Shiko.CourseReviewProvider.Api.Data;

namespace Shiko.CourseReviewProvider.Api.IntegrationTests.TestInfrastructure;

public sealed class CourseReviewApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public FakeCourseRatingClient CourseRatingClient { get; } = new();

    public CourseReviewApiFactory(string connectionString)
    {
        _connectionString = connectionString;

        Environment.SetEnvironmentVariable(
            "ConnectionStrings__CourseReviewDatabase",
            connectionString);

        Environment.SetEnvironmentVariable("Jwt__Issuer", "test-issuer");
        Environment.SetEnvironmentVariable("Jwt__Audience", "test-audience");
        Environment.SetEnvironmentVariable(
            "Jwt__SigningKey",
            "test-signing-key-for-integration-tests-123456789");

        Environment.SetEnvironmentVariable(
            "CourseRatingProvider__BaseUrl",
            "https://course-rating-provider.test");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<CourseReviewDbContext>();
            services.RemoveAll<DbContextOptions<CourseReviewDbContext>>();
            services.RemoveAll<ICourseRatingClient>();

            services.AddDbContext<CourseReviewDbContext>(options =>
            {
                options.UseSqlServer(
                    _connectionString,
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsHistoryTable(
                            "__EFMigrationsHistory",
                            "course_review");
                    });
            });

            services.AddSingleton<ICourseRatingClient>(CourseRatingClient);
        });
    }

    public async Task ApplyMigrationsAsync()
    {
        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<CourseReviewDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}