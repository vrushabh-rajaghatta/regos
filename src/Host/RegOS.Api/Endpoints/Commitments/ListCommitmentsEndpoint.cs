using RegOS.Interaction.Application.Queries.ListCommitments;

namespace RegOS.Api.Endpoints.Commitments;

public static class ListCommitmentsEndpoint
{
    public static IEndpointRouteBuilder MapListCommitments(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/commitments", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        ListCommitmentsHandler handler,
        CancellationToken cancellationToken,
        bool includeClosed = false)
    {
        var result = await handler.HandleAsync(
            new ListCommitmentsQuery(includeClosed), cancellationToken);

        return Results.Ok(result);
    }
}
