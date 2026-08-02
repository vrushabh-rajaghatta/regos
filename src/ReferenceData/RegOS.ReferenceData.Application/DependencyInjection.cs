using Microsoft.Extensions.DependencyInjection;

using RegOS.ReferenceData.Application.Queries.Geography.ListCountries;
using RegOS.ReferenceData.Application.Queries.Regulatory.ListAuthorities;
using RegOS.ReferenceData.Application.Queries.Regulatory.ListAuthorityDivisions;
using RegOS.ReferenceData.Application.Queries.Regulatory.ListCorrespondenceTypes;
using RegOS.ReferenceData.Application.Queries.ApplicationTypes.ListApplicationTypes;
using RegOS.ReferenceData.Application.Queries.SubmissionTypes.ListSubmissionTypes;
using RegOS.ReferenceData.Application.Queries.SubmissionSubTypes.ListSubmissionSubTypes;
using RegOS.ReferenceData.Application.Queries.DocumentTypes.ListDocumentTypes;
using RegOS.ReferenceData.Application.Queries.Blueprint.ListRegulatoryTemplates;
using RegOS.ReferenceData.Application.Queries.Blueprint.GetRegulatoryTemplate;
using RegOS.ReferenceData.Application.Queries.Organization.ListContactRoles;
using RegOS.ReferenceData.Application.Queries.Organization.ListIdentifierSchemes;

namespace RegOS.ReferenceData.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddReferenceDataApplication(
        this IServiceCollection services)
    {
        services.AddScoped<ListCountriesHandler>();
        services.AddScoped<ListAuthoritiesHandler>();

        services.AddScoped<ListCorrespondenceTypesHandler>();

        services.AddScoped<ListAuthorityDivisionsHandler>();
        services.AddScoped<ListApplicationTypesHandler>();
        services.AddScoped<ListSubmissionTypesHandler>();
        services.AddScoped<ListSubmissionSubTypesHandler>();
        services.AddScoped<ListDocumentTypesHandler>();
        services.AddScoped<ListRegulatoryTemplatesHandler>();
        services.AddScoped<GetRegulatoryTemplateHandler>();
        services.AddScoped<ListIdentifierSchemesHandler>();
        services.AddScoped<ListContactRolesHandler>();

        return services;
    }
}
