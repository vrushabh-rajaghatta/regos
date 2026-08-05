using Microsoft.Extensions.DependencyInjection;

using RegOS.Registration.Application.Services;
using RegOS.Registration.Domain.Aggregates.PackAuthorisations;
using RegOS.Registration.Domain.Aggregates.SiteApprovals;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.Registration.Infrastructure.Repositories;
using RegOS.Registration.Infrastructure.Services;

namespace RegOS.Registration.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRegistrationInfrastructure(
        this IServiceCollection services)
    {
        services.AddScoped<IRegistrationRepository, RegistrationRepository>();

        services.AddScoped<
            IPackAuthorisationRepository, PackAuthorisationRepository>();

        services.AddScoped<
            ISiteApprovalRepository, SiteApprovalRepository>();

        services.AddScoped<IRegistrationCreationPolicy, RegistrationCreationPolicy>();

        return services;
    }
}
