using GatewayApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace GatewayApi.Infrastructure;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    #region Methods

    public DbSet<User> Users => Set<User>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Item>(e =>
        {
            e.Property(x => x.Name)
                .HasMaxLength(80)
                .IsRequired();

            e.Property(x => x.Category)
                .HasMaxLength(40)
                .IsRequired();
        });
    }

    #endregion
}
