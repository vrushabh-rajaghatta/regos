using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using RegOS.Persistence.Initialization;
using RegOS.Persistence.Initialization.Organization;
using RegOS.Persistence.Initialization.Platform;
using RegOS.Persistence.Initialization.Product;
using RegOS.Persistence.Initialization.ReferenceData;
using RegOS.Persistence.Initialization.ReferenceData.Organization;
using RegOS.Persistence.Initialization.ReferenceData.Blueprint;

namespace RegOS.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<RegOSDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("RegOS")));

        services.AddScoped<IDataInitializer, GeographyAndRegulatoryInitializer>();
        services.AddScoped<IDataInitializer, OrganizationInitializer>();
        // Before ProductInitializer: products carry a tenant key, so the
        // tenants they point at must exist first.
        services.AddScoped<IDataInitializer, TenantInitializer>();
        services.AddScoped<IDataInitializer, ProductInitializer>();
        services.AddScoped<IDataInitializer, ApplicationTypeDataInitializer>();
        services.AddScoped<IDataInitializer, DocumentTypeDataInitializer>();
        services.AddScoped<IDataInitializer, IdentifierSchemeDataInitializer>();
        services.AddScoped<IDataInitializer, ContactRoleDataInitializer>();
        // Global, authority-independent — order relative to the others is free.
        services.AddScoped<IDataInitializer, CorrespondenceTypeDataInitializer>();
        // After GeographyAndRegulatoryInitializer: divisions reference authorities.
        services.AddScoped<IDataInitializer, AuthorityDivisionDataInitializer>();
        // After application types and authorities: a template references both.
        services.AddScoped<IDataInitializer, RegulatoryTemplateDataInitializer>();

        return services;
    }
}
