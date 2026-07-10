using Microsoft.EntityFrameworkCore;

public sealed class RegOSDbContext : DbContext
{
    public RegOSDbContext(DbContextOptions<RegOSDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}