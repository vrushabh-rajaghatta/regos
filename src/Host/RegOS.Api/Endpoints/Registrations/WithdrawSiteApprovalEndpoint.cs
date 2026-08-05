using RegOS.Registration.Application.Commands.WithdrawSiteApproval;
using RegOS.Registration.Domain.Aggregates.SiteApprovals;

namespace RegOS.Api.Endpoints.Registrations;

public static class WithdrawSiteApprovalEndpoint
{
    /// <remarks>
    /// <c>DELETE</c>, and the verb is the decision: this removes an approval
    /// recorded in error, not a site removed from a licence by variation. The
    /// second is an event with its own date and would be a field on the row.
    /// </remarks>
    public static IEndpointRouteBuilder MapWithdrawSiteApproval(
        this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/site-approvals/{siteApprovalId:guid}", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid siteApprovalId,
        WithdrawSiteApprovalHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new WithdrawSiteApprovalCommand(
                SiteApprovalId.From(siteApprovalId)),
            cancellationToken);

        return Results.NoContent();
    }
}
