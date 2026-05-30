using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shiko.CourseReviewProvider.Api.Data;
using Testcontainers.MsSql;
using Xunit;

namespace Shiko.CourseReviewProvider.Api.IntegrationTests.TestInfrastructure;

public sealed class CourseReviewIntegrationTestFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlServer = new MsSqlBuilder(
        "mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04")
        .Build();

    private CourseReviewApiFactory? _factory;

    public HttpClient Client { get; private set; } = null!;

    public IServiceProvider Services => _factory!.Services;

    public FakeCourseRatingClient CourseRatingClient => _factory!.CourseRatingClient;

    public async Task InitializeAsync()
    {
        await _sqlServer.StartAsync();

        _factory = new CourseReviewApiFactory(_sqlServer.GetConnectionString());

        await _factory.ApplyMigrationsAsync();

        Client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();

        _factory?.Dispose();

        await _sqlServer.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<CourseReviewDbContext>();

        dbContext.CourseReviews.RemoveRange(dbContext.CourseReviews);

        await dbContext.SaveChangesAsync();

        CourseRatingClient.UserHasRating = true;
    }
}