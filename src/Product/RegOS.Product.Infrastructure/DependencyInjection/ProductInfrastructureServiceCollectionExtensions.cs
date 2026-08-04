using Microsoft.Extensions.DependencyInjection;

using RegOS.Product.Application.Services;
using RegOS.Product.Domain.Product;
using RegOS.Product.Infrastructure.Persistence;
using RegOS.Product.Infrastructure.Services;

namespace RegOS.Product.Infrastructure.DependencyInjection;

public static class ProductInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddProductInfrastructure(
        this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductPolicy, ProductPolicy>();

        services.AddScoped<
            IMedicinalProductRepository, MedicinalProductRepository>();
        services.AddScoped<IMedicinalProductPolicy, MedicinalProductPolicy>();

        services.AddScoped<
            IPharmaceuticalProductDetailRepository,
            PharmaceuticalProductDetailRepository>();

        services.AddScoped<
            IMedicinalProductComponentRepository,
            MedicinalProductComponentRepository>();

        return services;
    }
}
