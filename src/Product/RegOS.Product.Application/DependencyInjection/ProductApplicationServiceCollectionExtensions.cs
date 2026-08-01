using Microsoft.Extensions.DependencyInjection;
using RegOS.Product.Application.Commands.AddTradeName;
using RegOS.Product.Application.Commands.ArchiveProduct;
using RegOS.Product.Application.Commands.ChangeMarketStatus;
using RegOS.Product.Application.Commands.CreateMedicinalProduct;
using RegOS.Product.Application.Commands.RegisterProduct;
using RegOS.Product.Application.Commands.RemoveTradeName;
using RegOS.Product.Application.Commands.UpdateProduct;
using RegOS.Product.Application.Queries.GetProduct;
using RegOS.Product.Application.Queries.ListMedicinalProducts;
using RegOS.Product.Application.Queries.ListProducts;

namespace RegOS.Product.Application.DependencyInjection;

public static class ProductApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddProductApplication(
        this IServiceCollection services)
    {
        services.AddScoped<RegisterProductHandler>();
        services.AddScoped<UpdateProductHandler>();
        services.AddScoped<ArchiveProductHandler>();
        services.AddScoped<GetProductHandler>();
        services.AddScoped<ListProductsHandler>();

        services.AddScoped<CreateMedicinalProductHandler>();
        services.AddScoped<ListMedicinalProductsHandler>();
        services.AddScoped<AddTradeNameHandler>();
        services.AddScoped<RemoveTradeNameHandler>();
        services.AddScoped<ChangeMarketStatusHandler>();

        return services;
    }
}