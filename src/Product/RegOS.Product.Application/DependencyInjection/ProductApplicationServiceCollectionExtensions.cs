using Microsoft.Extensions.DependencyInjection;
using RegOS.Product.Application.Commands.ActivateMedicinalProduct;
using RegOS.Product.Application.Commands.AddTradeName;
using RegOS.Product.Application.Commands.ArchiveProduct;
using RegOS.Product.Application.Commands.ChangeMarketStatus;
using RegOS.Product.Application.Commands.CreateMedicinalProduct;
using RegOS.Product.Application.Commands.DeactivateMedicinalProduct;
using RegOS.Product.Application.Commands.AddComponent;
using RegOS.Product.Application.Commands.AddIngredient;
using RegOS.Product.Application.Commands.AddPack;
using RegOS.Product.Application.Commands.AddPackageItem;
using RegOS.Product.Application.Commands.MovePackageItem;
using RegOS.Product.Application.Commands.RemovePackageItem;
using RegOS.Product.Application.Commands.RestatePackageItem;
using RegOS.Product.Application.Queries.ListPackageItems;
using RegOS.Product.Application.Commands.ChangePackMarketingStatus;
using RegOS.Product.Application.Commands.RestatePack;
using RegOS.Product.Application.Commands.StatePackSupply;
using RegOS.Product.Application.Queries.ListPacks;
using RegOS.Product.Application.Commands.MoveComponent;
using RegOS.Product.Application.Commands.RemoveComponent;
using RegOS.Product.Application.Commands.RestateComponent;
using RegOS.Product.Application.Commands.AddPresentation;
using RegOS.Product.Application.Commands.RemoveIngredient;
using RegOS.Product.Application.Commands.RestateIngredient;
using RegOS.Product.Application.Commands.RecordAtcCode;
using RegOS.Product.Application.Commands.RegisterProduct;
using RegOS.Product.Application.Commands.RemoveTradeName;
using RegOS.Product.Application.Commands.RestatePresentation;
using RegOS.Product.Application.Commands.UpdateProduct;
using RegOS.Product.Application.Queries.ListComponents;
using RegOS.Product.Application.Queries.ListProductsContainingSubstance;
using RegOS.Product.Application.Queries.ListPresentations;
using RegOS.Product.Application.Queries.GetProduct;
using RegOS.Product.Application.Queries.GetMedicinalProduct;
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
        services.AddScoped<GetMedicinalProductHandler>();
        services.AddScoped<AddTradeNameHandler>();
        services.AddScoped<RemoveTradeNameHandler>();
        services.AddScoped<ChangeMarketStatusHandler>();
        services.AddScoped<ActivateMedicinalProductHandler>();
        services.AddScoped<DeactivateMedicinalProductHandler>();
        services.AddScoped<RecordAtcCodeHandler>();

        services.AddScoped<AddPresentationHandler>();
        services.AddScoped<RestatePresentationHandler>();
        services.AddScoped<ListPresentationsHandler>();

        services.AddScoped<AddIngredientHandler>();
        services.AddScoped<RestateIngredientHandler>();
        services.AddScoped<RemoveIngredientHandler>();

        services.AddScoped<AddComponentHandler>();
        services.AddScoped<RestateComponentHandler>();
        services.AddScoped<MoveComponentHandler>();
        services.AddScoped<RemoveComponentHandler>();
        services.AddScoped<ListComponentsHandler>();

        services.AddScoped<AddPackHandler>();
        services.AddScoped<RestatePackHandler>();
        services.AddScoped<ChangePackMarketingStatusHandler>();
        services.AddScoped<StatePackSupplyHandler>();
        services.AddScoped<ListPacksHandler>();

        services.AddScoped<AddPackageItemHandler>();
        services.AddScoped<RestatePackageItemHandler>();
        services.AddScoped<MovePackageItemHandler>();
        services.AddScoped<RemovePackageItemHandler>();
        services.AddScoped<ListPackageItemsHandler>();

        // The capstone read: Substance -> Ingredient -> presentation -> market.
        services.AddScoped<ListProductsContainingSubstanceHandler>();

        return services;
    }
}