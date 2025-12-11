using AnalyticsApi.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnalyticsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly AnalyticsDbContext _db;

    public AnalyticsController(AnalyticsDbContext db)
    {
        _db = db;
    }

    [HttpGet("events/count")]
    public async Task<IActionResult> GetTotalEvents()
    {
        var total = await _db.Events.CountAsync();
        return Ok(new { totalEvents = total });
    }

    [HttpGet("events/by-type")]
    public async Task<IActionResult> GetEventsByType()
    {
        var result = await _db.Events
            .GroupBy(e => e.EventType)
            .Select(g => new { eventType = g.Key, count = g.Count() })
            .ToListAsync();

        return Ok(result);
    }
}