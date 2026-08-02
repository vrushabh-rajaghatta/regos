using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Application.Queries.GetApplicationContacts;

namespace RegOS.Api.Endpoints.RegulatoryApplications;

public static class GetApplicationContactsEndpoint
{
    public static IEndpointRouteBuilder MapGetApplicationContacts(
        this IEndpointRouteBuilder app)
    {
        // Who currently speaks for this application. Derived from the latest
        // published sequence rather than stored — there is deliberately no
        // application-level contact model (ADR-048).
        app.MapGet("/api/applications/{applicationId:guid}/contacts", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid applicationId,
        GetApplicationContactsHandler handler,
        CancellationToken cancellationToken)
    {
        var contacts = await handler.HandleAsync(
            new GetApplicationContactsQuery(
                new RegulatoryApplicationId(applicationId)),
            cancellationToken);

        return Results.Ok(contacts);
    }
}
