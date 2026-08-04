using RegOS.Labeling.Application.Commands.AddIndicationTherapy;
using RegOS.Labeling.Application.Commands.RemoveIndicationTherapy;
using RegOS.Labeling.Domain.Aggregates.Indications;

namespace RegOS.Api.Endpoints.Indications;

/// <summary>
/// Another therapy this authorisation is qualified by. No amend: the therapy is
/// one free-text phrase and a relationship, and correcting it is replacing it.
/// </summary>
public static class IndicationTherapyEndpoints
{
    public static IEndpointRouteBuilder MapIndicationTherapies(
        this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/indications/{indicationId:guid}/therapies", AddAsync);

        app.MapDelete(
            "/api/indications/{indicationId:guid}/therapies/{therapyId:guid}",
            RemoveAsync);

        return app;
    }

    private static async Task<IResult> AddAsync(
        Guid indicationId,
        TherapyRequest request,
        AddIndicationTherapyHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new AddIndicationTherapyCommand(
                IndicationId.From(indicationId),
                request.RelationshipCode,
                request.Therapy),
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> RemoveAsync(
        Guid indicationId,
        Guid therapyId,
        RemoveIndicationTherapyHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RemoveIndicationTherapyCommand(
                IndicationId.From(indicationId),
                OtherTherapyId.From(therapyId)),
            cancellationToken);

        return Results.NoContent();
    }
}
