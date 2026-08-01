using Microsoft.Extensions.DependencyInjection;

using RegOS.Interaction.Application.Services;
using RegOS.Interaction.Domain.Commitments;
using RegOS.Interaction.Domain.Correspondence;
using RegOS.Interaction.Domain.Inspections;
using RegOS.Interaction.Domain.Meetings;
using RegOS.Interaction.Infrastructure.Repositories;
using RegOS.Interaction.Infrastructure.Services;

namespace RegOS.Interaction.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInteractionInfrastructure(
        this IServiceCollection services)
    {
        services.AddScoped<IHaCorrespondenceRepository, HaCorrespondenceRepository>();

        services.AddScoped<IHaCorrespondencePolicy, HaCorrespondencePolicy>();

        services.AddScoped<ICommitmentRepository, CommitmentRepository>();

        services.AddScoped<IHaMeetingRepository, HaMeetingRepository>();

        services.AddScoped<IInspectionRepository, InspectionRepository>();

        return services;
    }
}
