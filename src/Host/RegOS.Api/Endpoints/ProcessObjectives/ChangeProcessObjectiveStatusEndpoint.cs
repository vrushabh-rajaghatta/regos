using RegOS.Process.Application.Commands.ChangeProcessObjectiveStatus;
using RegOS.Process.Domain.Aggregates.ProcessObjectives;

namespace RegOS.Api.Endpoints.ProcessObjectives;

public static class ChangeProcessObjectiveStatusEndpoint
{
    public static IEndpointRouteBuilder MapChangeProcessObjectiveStatusEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/process-objectives/{id:guid}/status", HandleAsync);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        ChangeProcessObjectiveStatusRequest request,
        ChangeProcessObjectiveStatusHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new ChangeProcessObjectiveStatusCommand(
                ProcessObjectiveId.From(id),
                request.Status,
                request.OccurredOn,
                request.Note),
            cancellationToken);

        return Results.NoContent();
    }

    public sealed record ChangeProcessObjectiveStatusRequest(
        ProcessObjectiveStatus Status,
        DateOnly OccurredOn,
        string? Note);
}
