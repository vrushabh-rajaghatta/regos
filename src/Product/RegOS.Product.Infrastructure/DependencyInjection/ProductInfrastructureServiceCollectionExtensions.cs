namespace RegOS.Product.Infrastructure.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using RegOS.Product.Application.Persistence;
using RegOS.Product.Contracts.Readers;
using RegOS.Product.Infrastructure.Persistence;
using RegOS.Product.Infrastructure.Readers;

public static class ProductInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddProductInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProductReader, ProductReader>();

        return services;
    }
}