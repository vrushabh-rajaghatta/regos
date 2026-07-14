using Microsoft.Extensions.DependencyInjection;

using RegOS.ReferenceData.Application.Queries.SubmissionTypes.ListSubmissionTypes;

namespace RegOS.ReferenceData.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddReferenceDataApplication(
        this IServiceCollection services)
    {
        services.AddScoped<ListSubmissionTypesHandler>();

        return services;
    }
}
