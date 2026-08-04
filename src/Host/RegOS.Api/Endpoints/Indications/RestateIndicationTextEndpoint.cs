using RegOS.Labeling.Application.Commands.RestateIndicationText;
using RegOS.Labeling.Domain.Aggregates.Indications;

namespace RegOS.Api.Endpoints.Indications;

public static class RestateIndicationTextEndpoint
{
    public static IEndpointRouteBuilder MapRestateIndicationText(
        this IEndpointRouteBuilder app)
    {
        // PUT: the wording is replaced, and replacing it changes nothing about
        // the authorisation — which is why this is not a "decision".
        app.MapPut("/api/indications/{indicationId:guid}/text", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid indicationId,
        RestateIndicationTextRequest request,
        RestateIndicationTextHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RestateIndicationTextCommand(
                IndicationId.From(indicationId), request.LabelText),
            cancellationToken);

        return Results.NoContent();
    }
}
