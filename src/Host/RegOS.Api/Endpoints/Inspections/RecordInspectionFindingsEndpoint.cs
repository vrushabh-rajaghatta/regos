using RegOS.Interaction.Application.Commands.RecordInspectionFindings;
using RegOS.Interaction.Domain.Inspections;

namespace RegOS.Api.Endpoints.Inspections;

public static class RecordInspectionFindingsEndpoint
{
    public static IEndpointRouteBuilder MapRecordInspectionFindings(
        this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/inspections/{inspectionId:guid}/findings", HandleAsync);

        return app;
    }

    private static async Task<IResult> HandleAsync(
        Guid inspectionId,
        RecordInspectionFindingsRequest request,
        RecordInspectionFindingsHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RecordInspectionFindingsCommand(
                InspectionId.From(inspectionId), request.Findings),
            cancellationToken);

        return Results.NoContent();
    }
}
