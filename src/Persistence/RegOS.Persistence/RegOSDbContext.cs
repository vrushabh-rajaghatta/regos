using Microsoft.EntityFrameworkCore;

using ProductAggregate = RegOS.Product.Domain.Product.Product;
using RegulatoryApplicationAggregate =
    RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;

namespace RegOS.Persistence;

public sealed class RegOSDbContext : DbContext
{
    public RegOSDbContext(
        DbContextOptions<RegOSDbContext> options)
        : base(options)
    {
    }

    public DbSet<ProductAggregate> Products =>
        Set<ProductAggregate>();

    public DbSet<RegulatoryApplicationAggregate> RegulatoryApplications =>
        Set<RegulatoryApplicationAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(RegOSDbContext).Assembly);
    }
}
