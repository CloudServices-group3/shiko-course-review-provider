using System.Net;
using System.Net.Http.Headers;

namespace Shiko.CourseReviewProvider.Api.Clients;

public sealed class CourseRatingClient(HttpClient httpClient) : ICourseRatingClient
{
    public async Task<bool> UserHasRatingAsync(
        Guid courseId,
        string accessToken,
        CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/course-ratings/{courseId}/me");

        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            accessToken);

        var response = await httpClient.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        response.EnsureSuccessStatusCode();

        return true;
    }
}