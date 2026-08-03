using Microsoft.Extensions.DependencyInjection;

using RegOS.Study.Application.Commands.RegisterClinicalStudy;
using RegOS.Study.Application.Commands.RegisterNonClinicalStudy;
using RegOS.Study.Application.Queries.ListStudies;

namespace RegOS.Study.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddStudyApplication(
        this IServiceCollection services)
    {
        services.AddScoped<RegisterClinicalStudyHandler>();

        services.AddScoped<RegisterNonClinicalStudyHandler>();

        services.AddScoped<ListStudiesHandler>();

        return services;
    }
}
