using RegOS.Interaction.Application.Commands.ChangeInspectionStatus;
using RegOS.Interaction.Domain.Inspections;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Api.Endpoints.Inspections;

public static class ChangeInspectionStatusEndpoint
{
    public static IEndpointRouteBuilder MapChangeInspectionStatus(
        this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/inspections/{inspectionId:guid}/status", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid inspectionId,
        ChangeInspectionStatusRequest request,
        ChangeInspectionStatusHandler handler,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<InspectionStatus>(
                request.Status, ignoreCase: true, out var target))
        {
            throw new DomainException(
                "Status must be one of InProgress, Completed or Cancelled.");
        }

        await handler.HandleAsync(
            new ChangeInspectionStatusCommand(
                InspectionId.From(inspectionId),
                target,
                request.OccurredOn,
                request.Note),
            cancellationToken);

        return Results.NoContent();
    }
}
