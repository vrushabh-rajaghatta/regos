using Microsoft.EntityFrameworkCore;

using RegulatoryApplicationAggregate = RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;

namespace RegOS.RegulatoryApplication.Infrastructure.Persistence;

public sealed class RegulatoryApplicationDbContext : DbContext
{
    public RegulatoryApplicationDbContext(
        DbContextOptions<RegulatoryApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<RegulatoryApplicationAggregate> RegulatoryApplications =>
        Set<RegulatoryApplicationAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(RegulatoryApplicationDbContext).Assembly);
    }
}
