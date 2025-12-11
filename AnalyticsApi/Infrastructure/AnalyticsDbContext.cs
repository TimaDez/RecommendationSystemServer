using Microsoft.EntityFrameworkCore;

namespace AnalyticsApi.Infrastructure;

public class AnalyticsDbContext : DbContext
{
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options)
        : base(options)
    {
    }

    public DbSet<EventEntity> Events => Set<EventEntity>();
}

public class EventEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ItemId { get; set; }
    public string EventType { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}