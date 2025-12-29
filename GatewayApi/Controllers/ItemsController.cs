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
    #region Private members

    private readonly AppDbContext _db;

    #endregion

    #region Methods

    public ItemsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<ItemResponse>> Create([FromBody] CreateItemRequest request)
    {
        Console.WriteLine($"SERVER received JSON -> DTO: {System.Text.Json.JsonSerializer.Serialize(request)}");

        var item = new Item
        {
            Name = request.Name,
            Category = request.Category
        };

        _db.Items.Add(item);
        await _db.SaveChangesAsync();

        var response = new ItemResponse(
            item.Id,
            item.Name,
            item.Category
        );

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, response);
    }

    [HttpPut("{id:int}")] // ✅ NEW
    public async Task<ActionResult<ItemResponse>> Update(int id, [FromBody] UpdateItemRequest request) // ✅ NEW
    {
        var item = await _db.Items.FindAsync(id); // ✅ NEW
        if (item is null) // ✅ NEW
        {
            return NotFound();
        }

        item.Name = request.Name; // ✅ NEW
        item.Category = request.Category; // ✅ NEW

        await _db.SaveChangesAsync(); // ✅ NEW

        var response = new ItemResponse( // ✅ NEW
            item.Id,
            item.Name,
            item.Category
        );

        return Ok(response); // ✅ NEW
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ItemResponse>> GetById(int id)
    {
        var item = await _db.Items.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        var response = new ItemResponse(
            item.Id,
            item.Name,
            item.Category
        );

        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemResponse>>> GetAll()
    {
        var items = await _db.Items
            .OrderBy(i => i.Id)
            .Select(i => new ItemResponse(
                i.Id,
                i.Name,
                i.Category
            ))
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

    #endregion
}
