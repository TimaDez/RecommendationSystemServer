using GatewayApi.Contracts;
using GatewayApi.Domain;
using GatewayApi.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GatewayApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ItemsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<ItemResponse>> Create([FromBody] CreateItemRequest request)
    {
        var item = new Item
        {
            Name = request.Name,
            Category = request.Category
        };

        _db.Items.Add(item);
        await _db.SaveChangesAsync();

        var response = new ItemResponse
        {
            Id = item.Id,
            Name = item.Name,
            Category = item.Category
        };

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ItemResponse>> GetById(int id)
    {
        var item = await _db.Items.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        var response = new ItemResponse
        {
            Id = item.Id,
            Name = item.Name,
            Category = item.Category
        };

        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemResponse>>> GetAll()
    {
        var items = await _db.Items
            .OrderBy(i => i.Id)
            .Select(i => new ItemResponse
            {
                Id = i.Id,
                Name = i.Name,
                Category = i.Category
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Items.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        _db.Items.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
