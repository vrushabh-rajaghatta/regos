using Microsoft.Extensions.DependencyInjection;

using RegOS.Study.Application.Services;
using RegOS.Study.Domain.Aggregates.ClinicalStudy;
using RegOS.Study.Domain.Aggregates.NonClinicalStudy;
using RegOS.Study.Infrastructure.Repositories;
using RegOS.Study.Infrastructure.Services;

namespace RegOS.Study.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddStudyInfrastructure(
        this IServiceCollection services)
    {
        services.AddScoped<IClinicalStudyRepository, ClinicalStudyRepository>();

        services.AddScoped<
            INonClinicalStudyRepository, NonClinicalStudyRepository>();

        services.AddScoped<
            ISponsorStudyIdentifierPolicy, SponsorStudyIdentifierPolicy>();

        return services;
    }
}
