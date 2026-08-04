using Microsoft.Extensions.DependencyInjection;

using RegOS.ReferenceData.Domain.Substances;
using RegOS.ReferenceData.Infrastructure.Repositories;

namespace RegOS.ReferenceData.Infrastructure;

/// <summary>
/// <c>ReferenceData</c>'s first Infrastructure project, added as the ordinary
/// shape every other context already has rather than as an exception — a
/// special case is a thing the next contributor copies (ADR-058 §4).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddReferenceDataInfrastructure(
        this IServiceCollection services)
    {
        services.AddScoped<ISubstanceRepository, SubstanceRepository>();

        return services;
    }
}
