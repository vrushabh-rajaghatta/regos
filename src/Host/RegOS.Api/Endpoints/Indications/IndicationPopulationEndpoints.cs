using RegOS.Labeling.Application.Commands.AddIndicationPopulation;
using RegOS.Labeling.Application.Commands.AmendIndicationPopulation;
using RegOS.Labeling.Application.Commands.RemoveIndicationPopulation;
using RegOS.Labeling.Domain.Aggregates.Indications;

namespace RegOS.Api.Endpoints.Indications;

/// <summary>
/// Add, amend and remove — the three operations EPIC-018 D2 is judged on.
/// </summary>
/// <remarks>
/// <b>Amend is a PUT on one population, not a replace of the collection</b>,
/// and that is the whole point: a band written as 2–12 and corrected to 2–11 is
/// the same qualifier, and the id survives the correction.
/// <para>
/// Three routes in one file, so the capability reads as one thing. Each maps a
/// named method per SC-004.
/// </para>
/// </remarks>
public static class IndicationPopulationEndpoints
{
    public static IEndpointRouteBuilder MapIndicationPopulations(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/indications/{indicationId:guid}/populations", AddAsync);

        app.MapPut(
            "/api/indications/{indicationId:guid}/populations/{populationId:guid}",
            AmendAsync);

        app.MapDelete(
            "/api/indications/{indicationId:guid}/populations/{populationId:guid}",
            RemoveAsync);

        return app;
    }

    private static async Task<IResult> AddAsync(
        Guid indicationId,
        PopulationRequest request,
        AddIndicationPopulationHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new AddIndicationPopulationCommand(
                IndicationId.From(indicationId),
                request.AgeLow,
                request.AgeHigh,
                request.AgeUnitCode,
                request.GenderCode,
                request.PhysiologicalConditionCode,
                request.Description),
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> AmendAsync(
        Guid indicationId,
        Guid populationId,
        PopulationRequest request,
        AmendIndicationPopulationHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new AmendIndicationPopulationCommand(
                IndicationId.From(indicationId),
                PopulationId.From(populationId),
                request.AgeLow,
                request.AgeHigh,
                request.AgeUnitCode,
                request.GenderCode,
                request.PhysiologicalConditionCode,
                request.Description),
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> RemoveAsync(
        Guid indicationId,
        Guid populationId,
        RemoveIndicationPopulationHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RemoveIndicationPopulationCommand(
                IndicationId.From(indicationId),
                PopulationId.From(populationId)),
            cancellationToken);

        return Results.NoContent();
    }
}
