using GatewayApi.Contracts;
using GatewayApi.Domain;
using GatewayApi.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GatewayApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly AppDbContext _db;

    public EventsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<EventResponse>> Create([FromBody] CreateEventRequest request)
    {
        var userExists = await _db.Users.AnyAsync(u => u.Id == request.UserId);
        var itemExists = await _db.Items.AnyAsync(i => i.Id == request.ItemId);

        if (!userExists || !itemExists)
        {
            return BadRequest("User or Item does not exist.");
        }

        var ev = new Event
        {
            UserId = request.UserId,
            ItemId = request.ItemId,
            EventType = request.EventType,
            Timestamp = DateTime.UtcNow
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        var response = new EventResponse
        {
            Id = ev.Id,
            UserId = ev.UserId,
            ItemId = ev.ItemId,
            EventType = ev.EventType,
            Timestamp = ev.Timestamp
        };

        return CreatedAtAction(nameof(GetById), new { id = ev.Id }, response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventResponse>> GetById(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev is null)
        {
            return NotFound();
        }

        var response = new EventResponse
        {
            Id = ev.Id,
            UserId = ev.UserId,
            ItemId = ev.ItemId,
            EventType = ev.EventType,
            Timestamp = ev.Timestamp
        };

        return Ok(response);
    }

    [HttpGet("by-user/{userId:int}")]
    public async Task<ActionResult<IEnumerable<EventResponse>>> GetByUser(int userId)
    {
        var events = await _db.Events
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.Timestamp)
            .Select(e => new EventResponse
            {
                Id = e.Id,
                UserId = e.UserId,
                ItemId = e.ItemId,
                EventType = e.EventType,
                Timestamp = e.Timestamp
            })
            .ToListAsync();

        return Ok(events);
    }
}
