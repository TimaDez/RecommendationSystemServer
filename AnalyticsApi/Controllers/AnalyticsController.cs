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

    // 1) כמה קליקים לכל קטגוריה עבור משתמש מסוים
    // GET: /api/analytics/user/1/clicks-by-category
    [HttpGet("user/{userId:int}/clicks-by-category")]
    public async Task<IActionResult> GetUserClicksByCategory(int userId, CancellationToken cancellationToken)
    {
        var query =
            from e in _db.Events
            join i in _db.Items on e.ItemId equals i.Id
            where e.UserId == userId && e.EventType == "click"
            group e by i.Category into g
            orderby g.Key
            select new
            {
                Category = g.Key,
                ClickCount = g.Count()
            };

        var result = await query.ToListAsync(cancellationToken);

        return Ok(result);
    }

    // 2) הטופ N אייטמים לפי כמות אירועים מסוג מסוים
    // GET: /api/analytics/top-items?eventType=view&limit=10
    [HttpGet("top-items")]
    public async Task<IActionResult> GetTopItems(
        [FromQuery] string eventType = "view",
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (limit <= 0 || limit > 100)
            limit = 10;

        var query =
            from e in _db.Events
            join i in _db.Items on e.ItemId equals i.Id
            where e.EventType == eventType
            group e by new { i.Id, i.Name, i.Category } into g
            orderby g.Count() descending
            select new
            {
                ItemId = g.Key.Id,
                ItemName = g.Key.Name,
                Category = g.Key.Category,
                EventType = eventType,
                EventCount = g.Count()
            };

        var result = await query.Take(limit).ToListAsync(cancellationToken);

        return Ok(result);
    }
}
