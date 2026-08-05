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
        // After GeographyAndRegulatoryInitializer: both reference authorities.
        // Independent of each other — a sub-type is not a taxonomy beneath a
        // type (ADR-047 §6), so neither ordering would mean anything.
        services.AddScoped<IDataInitializer, SubmissionTypeDataInitializer>();
        services.AddScoped<IDataInitializer, SubmissionSubTypeDataInitializer>();
        services.AddScoped<IDataInitializer, DocumentTypeDataInitializer>();
        services.AddScoped<IDataInitializer, IdentifierSchemeDataInitializer>();
        services.AddScoped<IDataInitializer, ContactRoleDataInitializer>();

        // After IdentifierSchemeDataInitializer above, and that ordering is
        // load-bearing: a demo site carries an FEI and a DUNS, and the scheme
        // rows those name must exist first.
        services.AddScoped<IDataInitializer, SiteInitializer>();
        // Global, authority-independent — order relative to the others is free.
        services.AddScoped<IDataInitializer, CorrespondenceTypeDataInitializer>();
        // After GeographyAndRegulatoryInitializer: divisions reference authorities.
        services.AddScoped<IDataInitializer, AuthorityDivisionDataInitializer>();
        // References nothing — a substance is a fact about the world, which is
        // why it depends on no other seed and no tenant.
        services.AddScoped<IDataInitializer, SubstanceDataInitializer>();
        // After application types and authorities: a template references both.
        services.AddScoped<IDataInitializer, RegulatoryTemplateDataInitializer>();

        return services;
    }
}
