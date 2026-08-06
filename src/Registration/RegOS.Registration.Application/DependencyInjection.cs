using Microsoft.Extensions.DependencyInjection;

using RegOS.Registration.Application.Commands.ApproveSite;
using RegOS.Registration.Application.Commands.AuthorisePack;
using RegOS.Registration.Application.Commands.WithdrawSiteApproval;
using RegOS.Registration.Application.Queries.ListApprovedSites;
using RegOS.Registration.Application.Queries.ListSiteAlignment;
using RegOS.Registration.Application.Commands.ChangeRegistrationStatus;
using RegOS.Registration.Application.Commands.CreateRegistration;
using RegOS.Registration.Application.Commands.RecordRegistrationApproval;
using RegOS.Registration.Application.Commands.WithdrawPackAuthorisation;
using RegOS.Registration.Application.Queries.ListAuthorisedPacks;
using RegOS.Registration.Application.Queries.GetRegistration;
using RegOS.Registration.Application.Queries.ListExpiringRegistrations;
using RegOS.Registration.Application.Queries.ListMarketRegistrations;
using RegOS.Registration.Application.Queries.ListProductRegistrations;
using RegOS.Registration.Application.Queries.ListRegistrationMarkets;

using RegOS.Registration.Application.Commands.AttachRegistrationToStep;

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

        services.AddScoped<ListExpiringRegistrationsHandler>();

        services.AddScoped<AuthorisePackHandler>();
        services.AddScoped<ApproveSiteHandler>();
        services.AddScoped<WithdrawSiteApprovalHandler>();
        services.AddScoped<ListApprovedSitesHandler>();
        services.AddScoped<ListSiteAlignmentHandler>();

        services.AddScoped<WithdrawPackAuthorisationHandler>();

        services.AddScoped<ListAuthorisedPacksHandler>();

        services.AddScoped<AttachRegistrationToStepHandler>();

        return services;
    }
}
