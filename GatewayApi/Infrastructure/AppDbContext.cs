using GatewayApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace GatewayApi.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Event> Events => Set<Event>();
}
