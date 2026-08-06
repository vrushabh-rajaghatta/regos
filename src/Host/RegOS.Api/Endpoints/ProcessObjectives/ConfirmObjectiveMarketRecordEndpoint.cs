using RegOS.Process.Application.Commands.ConfirmObjectiveMarketRecord;
using RegOS.Process.Domain.Aggregates.ProcessObjectives;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.ProcessObjectives;

public static class ConfirmObjectiveMarketRecordEndpoint
{
    public static IEndpointRouteBuilder MapConfirmObjectiveMarketRecordEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(
            "/api/process-objectives/{id:guid}/market-record", HandleAsync);

        return endpoints;
    }

    /// <summary>
    /// PUT, not POST: naming which market record fulfils an objective is setting
    /// a value, and sending null clears it. The handler refuses a record whose
    /// product and country are not this objective's (ADR-065 D8) and the
    /// middleware maps that to 409.
    /// </summary>
    private static async Task<IResult> HandleAsync(
        Guid id,
        ConfirmObjectiveMarketRecordRequest request,
        ConfirmObjectiveMarketRecordHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ConfirmObjectiveMarketRecordCommand(
                ProcessObjectiveId.From(id),
                request.MedicinalProductId is { } marketRecord
                    ? new MedicinalProductId(marketRecord)
                    : null),
            cancellationToken);

        return Results.NoContent();
    }

    public sealed record ConfirmObjectiveMarketRecordRequest(
        Guid? MedicinalProductId);
}
