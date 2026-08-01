using RegOS.Interaction.Application.Queries.ListDueWork;
using RegOS.Platform.Application.Services;
using RegOS.Platform.Contracts;

namespace RegOS.Api.Endpoints.Commitments;

public static class ListDueWorkEndpoint
{
    public static IEndpointRouteBuilder MapListDueWork(
        this IEndpointRouteBuilder app)
    {
        // The epic's headline read: "what is due, to whom, and when?"
        app.MapGet("/api/due-work", HandleAsync);

        return app;
    }

    /// <remarks>
    /// <b>"Mine" is resolved here, not in the handler.</b> The Host already
    /// knows who is asking; letting the Interaction context ask would make it
    /// depend on <c>Platform.Application</c> for a filter value, which is a
    /// worse edge than the one ADR-041 went to some trouble to avoid.
    /// </remarks>
    private static async Task<IResult> HandleAsync(
        ListDueWorkHandler handler,
        ICurrentUser currentUser,
        CancellationToken cancellationToken,
        bool mine = false,
        DateOnly? dueOnOrBefore = null)
    {
        var owner = mine ? UserId.From(currentUser.UserId) : null;

        var result = await handler.HandleAsync(
            new ListDueWorkQuery(owner, dueOnOrBefore), cancellationToken);

        return Results.Ok(result);
    }
}
