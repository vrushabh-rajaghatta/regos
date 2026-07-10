namespace RegOS.Product.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using RegOS.Product.Domain.Product;

public sealed class ProductDbContext : DbContext
{
    public ProductDbContext(
        DbContextOptions<ProductDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductDbContext).Assembly);
    }
}