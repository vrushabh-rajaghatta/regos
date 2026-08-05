using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.Registration.Application.Commands.ApproveSite;
using RegOS.Registration.Domain.Aggregates.Registration;

namespace RegOS.Api.Endpoints.Registrations;

public static class ApproveSiteEndpoint
{
    /// <remarks>
    /// Nested under the licence, because the licence is what makes the
    /// statement — unlike the read beside it, which is nested under the market,
    /// because that is who asks.
    /// </remarks>
    public static IEndpointRouteBuilder MapApproveSite(
        this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/registrations/{registrationId:guid}/sites", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid registrationId,
        ApproveSiteRequest request,
        ApproveSiteHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ApproveSiteCommand(
                new RegistrationId(registrationId),
                OrganizationSiteId.From(request.OrganizationSiteId),
                request.ApprovedOn),
            cancellationToken);

        return Results.Ok(new ApproveSiteResponse(result.SiteApprovalId));
    }
}
