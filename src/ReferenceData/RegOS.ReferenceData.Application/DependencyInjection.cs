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
using RegOS.ReferenceData.Application.Queries.Substances.ListSubstances;
using RegOS.ReferenceData.Application.Queries.Substances.GetSubstanceVocabulary;
using RegOS.ReferenceData.Application.Queries.Presentations.GetPharmaceuticalVocabulary;
using RegOS.ReferenceData.Application.Queries.Measurement.ListMeasurementUnits;
using RegOS.ReferenceData.Application.Queries.Labels.GetLabelVocabulary;
using RegOS.ReferenceData.Application.Queries.Clinical.GetClinicalVocabulary;
using RegOS.ReferenceData.Application.Commands.CreateSubstance;

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

        services.AddScoped<ListSubstancesHandler>();
        services.AddScoped<GetSubstanceVocabularyHandler>();
        services.AddScoped<GetPharmaceuticalVocabularyHandler>();
        services.AddScoped<GetLabelVocabularyHandler>();
        services.AddScoped<GetClinicalVocabularyHandler>();
        services.AddScoped<ListMeasurementUnitsHandler>();

        // The context's first command handler (ADR-058 §5). Everything above
        // it reads; this one writes, and only one thing — a tenant-owned
        // substance.
        services.AddScoped<CreateSubstanceHandler>();

        return services;
    }
}
