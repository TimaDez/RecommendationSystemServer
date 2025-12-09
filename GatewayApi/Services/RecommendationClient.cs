using System.Net.Http.Json;
using GatewayApi.Contracts;

namespace GatewayApi.Services;

public class RecommendationClient
{
    private readonly HttpClient _httpClient;

    public RecommendationClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<RecommendationResponseDto?> GetRecommendationsAsync(
        int userId,
        IReadOnlyList<UserEventForRecommendationDto> events,
        CancellationToken cancellationToken = default)
    {
        var request = new RecommendationRequestDto
        {
            UserId = userId,
            Events = events.ToList()
        };

        var response = await _httpClient.PostAsJsonAsync("/recommend", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<RecommendationResponseDto>(
            cancellationToken: cancellationToken);
    }
}