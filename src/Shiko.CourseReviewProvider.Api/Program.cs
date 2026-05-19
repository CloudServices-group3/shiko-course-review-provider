using Shiko.CourseReviewProvider.Api.Services;
using Shiko.CourseReviewProvider.Api.Clients;
using Shiko.CourseReviewProvider.Api.Data;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddDbContext<CourseReviewDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("CourseReviewDatabase"));
});

builder.Services.AddScoped<ICourseReviewService, CourseReviewService>();

builder.Services.AddHttpClient<ICourseRatingClient, CourseRatingClient>(client =>
{
    var baseUrl = builder.Configuration["CourseRatingProvider:BaseUrl"];

    if (string.IsNullOrWhiteSpace(baseUrl))
    {
        throw new InvalidOperationException("CourseRatingProvider:BaseUrl is not configured.");
    }

    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
