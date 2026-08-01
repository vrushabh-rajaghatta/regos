using RegOS.Interaction.Application.Queries.GetCorrespondence;
using RegOS.Interaction.Domain.Correspondence;

namespace RegOS.Api.Endpoints.Correspondence;

public static class GetCorrespondenceEndpoint
{
    public static IEndpointRouteBuilder MapGetCorrespondence(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/correspondence/{correspondenceId:guid}", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid correspondenceId,
        GetCorrespondenceHandler handler,
        CancellationToken cancellationToken)
    {
        // NotFoundException maps to 404 in middleware — endpoints do not catch
        // (ADR-012).
        var result = await handler.HandleAsync(
            new GetCorrespondenceQuery(HaCorrespondenceId.From(correspondenceId)),
            cancellationToken);

        return Results.Ok(result);
    }
}
