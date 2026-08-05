using RegOS.Labeling.Application.Commands.PrintLocalLabelForPack;
using RegOS.Labeling.Domain.Aggregates.LocalLabels;

namespace RegOS.Api.Endpoints.LocalLabels;

public static class PrintLocalLabelForPackEndpoint
{
    /// <remarks>
    /// On the label rather than on a revision: which pack a carton is printed
    /// for is what the document <em>is</em>, and revising the words on it does
    /// not make it a different pack's carton.
    /// </remarks>
    public static IEndpointRouteBuilder MapPrintLocalLabelForPack(
        this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/local-labels/{localLabelId:guid}/pack", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid localLabelId,
        PrintLocalLabelForPackRequest request,
        PrintLocalLabelForPackHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new PrintLocalLabelForPackCommand(
                LocalLabelId.From(localLabelId),
                request.PackagedProductId),
            cancellationToken);

        return Results.NoContent();
    }
}
