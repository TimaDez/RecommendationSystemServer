using AnalyticsApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace AnalyticsApi.Infrastructure;

public class AnalyticsDbContext : DbContext
{
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // וידוא שמות טבלאות תואמים למה שקיים בדאטהבייס
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Item>().ToTable("Items");
        modelBuilder.Entity<Event>().ToTable("Events");
    }
}