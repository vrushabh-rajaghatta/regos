using Microsoft.Extensions.DependencyInjection;

using RegOS.ReferenceData.Application.Queries.Geography.ListCountries;
using RegOS.ReferenceData.Application.Queries.Regulatory.ListAuthorities;
using RegOS.ReferenceData.Application.Queries.SubmissionTypes.ListSubmissionTypes;
using RegOS.ReferenceData.Application.Queries.DocumentTypes.ListDocumentTypes;

namespace RegOS.ReferenceData.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddReferenceDataApplication(
        this IServiceCollection services)
    {
        services.AddScoped<ListCountriesHandler>();
        services.AddScoped<ListAuthoritiesHandler>();
        services.AddScoped<ListSubmissionTypesHandler>();
        services.AddScoped<ListDocumentTypesHandler>();

        return services;
    }
}
