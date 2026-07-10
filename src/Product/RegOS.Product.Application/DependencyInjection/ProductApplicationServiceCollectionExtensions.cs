using Microsoft.Extensions.DependencyInjection;
using RegOS.Product.Application.Commands.RegisterProduct;

namespace RegOS.Product.Application.DependencyInjection;

public static class ProductApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddProductApplication(
        this IServiceCollection services)
    {
        services.AddScoped<RegisterProductHandler>();

        return services;
    }
}