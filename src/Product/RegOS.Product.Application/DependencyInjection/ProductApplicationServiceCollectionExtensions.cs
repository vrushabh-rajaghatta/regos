using Microsoft.Extensions.DependencyInjection;
using RegOS.Product.Application.Commands.RegisterProduct;
using RegOS.Product.Application.Queries.GetProduct;
using RegOS.Product.Application.Queries.ListProducts;

namespace RegOS.Product.Application.DependencyInjection;

public static class ProductApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddProductApplication(
        this IServiceCollection services)
    {
        services.AddScoped<RegisterProductHandler>();
        services.AddScoped<GetProductHandler>();
        services.AddScoped<ListProductsHandler>();

        return services;
    }
}