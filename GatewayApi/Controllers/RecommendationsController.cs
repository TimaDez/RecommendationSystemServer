using GatewayApi.Contracts;
using GatewayApi.Infrastructure;
using GatewayApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GatewayApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendationsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly RecommendationClient _client;

    public RecommendationsController(AppDbContext db, RecommendationClient client)
    {
        _db = db;
        _client = client;
    }

    [HttpGet("{userId:int}")]
    public async Task<ActionResult<RecommendationDetailedResponseDto>> Get(
        int userId,
        CancellationToken cancellationToken)
    {
        // 1. Get recent events for this user
        var recentEvents = await _db.Events
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Timestamp)
            .Take(50)
            .Select(e => new UserEventForRecommendationDto
            {
                ItemId = e.ItemId,
                EventType = e.EventType,
                Timestamp = e.Timestamp
            })
            .ToListAsync(cancellationToken);

        // If user has no events, return empty recommendations
        if (recentEvents.Count == 0)
        {
            return Ok(new RecommendationDetailedResponseDto
            {
                UserId = userId,
                Recommendations = new List<RecommendationItemDetailedDto>()
            });
        }

        // 2. Call Python AI service with these events
        var aiResponse = await _client.GetRecommendationsAsync(userId, recentEvents, cancellationToken);

        if (aiResponse is null || aiResponse.Recommendations.Count == 0)
        {
            return Ok(new RecommendationDetailedResponseDto
            {
                UserId = userId,
                Recommendations = new List<RecommendationItemDetailedDto>()
            });
        }

        // 3. Collect item IDs from AI response
        var recommendedItemIds = aiResponse.Recommendations
            .Select(r => r.ItemId)
            .Distinct()
            .ToList();

        // 4. Load item details from DB
        var items = await _db.Items
            .Where(i => recommendedItemIds.Contains(i.Id))
            .ToListAsync(cancellationToken);

        var itemsById = items.ToDictionary(i => i.Id);

        // 5. Load all events for this user and the recommended items
        var eventsForRecommendedItems = await _db.Events
            .Where(e => e.UserId == userId && recommendedItemIds.Contains(e.ItemId))
            .ToListAsync(cancellationToken);

        // Group events by item and count each event type
        var eventsByItemId = eventsForRecommendedItems
            .GroupBy(e => e.ItemId)
            .ToDictionary(
                g => g.Key,
                g => g
                    .GroupBy(ev => ev.EventType)
                    .ToDictionary(
                        gg => gg.Key,     // event type ("view", "click", "like", ...)
                        gg => gg.Count()  // how many times
                    )
            );

        // 6. Build detailed response
        var detailedRecommendations = new List<RecommendationItemDetailedDto>();

        foreach (var rec in aiResponse.Recommendations)
        {
            itemsById.TryGetValue(rec.ItemId, out var item);
            eventsByItemId.TryGetValue(rec.ItemId, out var eventCounts);

            detailedRecommendations.Add(new RecommendationItemDetailedDto
            {
                ItemId = rec.ItemId,
                ItemName = item?.Name ?? string.Empty,
                Category = item?.Category ?? string.Empty,
                Score = rec.Score,
                Reason = string.IsNullOrEmpty(rec.Reason)
                    ? "based on your activity"
                    : rec.Reason,
                EventCounts = eventCounts ?? new Dictionary<string, int>()
            });
        }

        var result = new RecommendationDetailedResponseDto
        {
            UserId = userId,
            Recommendations = detailedRecommendations
        };

        return Ok(result);
    }
}
