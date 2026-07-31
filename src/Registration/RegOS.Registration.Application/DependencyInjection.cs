using Microsoft.Extensions.DependencyInjection;

using RegOS.Registration.Application.Commands.ChangeRegistrationStatus;
using RegOS.Registration.Application.Commands.CreateRegistration;
using RegOS.Registration.Application.Commands.RecordRegistrationApproval;
using RegOS.Registration.Application.Queries.GetRegistration;
using RegOS.Registration.Application.Queries.ListMarketRegistrations;
using RegOS.Registration.Application.Queries.ListProductRegistrations;
using RegOS.Registration.Application.Queries.ListRegistrationMarkets;

namespace RegOS.Registration.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddRegistrationApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateRegistrationHandler>();

        services.AddScoped<RecordRegistrationApprovalHandler>();

        services.AddScoped<ChangeRegistrationStatusHandler>();

        services.AddScoped<GetRegistrationHandler>();

        services.AddScoped<ListProductRegistrationsHandler>();

        services.AddScoped<ListMarketRegistrationsHandler>();

        services.AddScoped<ListRegistrationMarketsHandler>();

        return services;
    }
}
