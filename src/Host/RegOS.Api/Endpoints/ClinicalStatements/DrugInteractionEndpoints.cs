using RegOS.Labeling.Application.Commands.AddDrugInteractionPopulation;
using RegOS.Labeling.Application.Commands.AddInteractant;
using RegOS.Labeling.Application.Commands.AmendDrugInteractionPopulation;
using RegOS.Labeling.Application.Commands.RecordDrugInteraction;
using RegOS.Labeling.Application.Commands.RemoveDrugInteractionPopulation;
using RegOS.Labeling.Application.Commands.RemoveInteractant;
using RegOS.Labeling.Application.Queries.ListDrugInteractions;
using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;
using RegOS.Labeling.Domain.Aggregates.DrugInteractions;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Substances;

namespace RegOS.Api.Endpoints.ClinicalStatements;

/// <param name="Interactant">
/// Required. An interaction with nothing to interact with is not a statement.
/// </param>
/// <param name="InteractantSubstanceId">
/// Optional — the seam that turns "which of our products interact with
/// warfarin?" into a join.
/// </param>
public sealed record RecordInteractionRequest(
    string InteractionTypeCode,
    string LabelText,
    string Interactant,
    Guid? InteractantSubstanceId,
    string? Management,
    string? SeverityCode);

public sealed record InteractantRequest(
    string Description,
    Guid? SubstanceId);

/// <summary>
/// The fourth clinical statement, and the same five capabilities as the others
/// plus the two its at-least-one invariant needs.
/// </summary>
public static class DrugInteractionEndpoints
{
    public static IEndpointRouteBuilder MapDrugInteractions(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/medicinal-products/{medicinalProductId:guid}/interactions",
            ListAsync);

        app.MapPost(
            "/api/medicinal-products/{medicinalProductId:guid}/interactions",
            RecordAsync);

        app.MapPost(
            "/api/interactions/{interactionId:guid}/interactants",
            AddInteractantAsync);

        // The aggregate refuses to remove the last one, so this cannot leave an
        // interaction with nothing to interact with.
        app.MapDelete(
            "/api/interactions/{interactionId:guid}/interactants/{interactantId:guid}",
            RemoveInteractantAsync);

        app.MapPost(
            "/api/interactions/{interactionId:guid}/populations", AddAsync);

        app.MapPut(
            "/api/interactions/{interactionId:guid}/populations/{populationId:guid}",
            AmendAsync);

        app.MapDelete(
            "/api/interactions/{interactionId:guid}/populations/{populationId:guid}",
            RemoveAsync);

        return app;
    }

    private static async Task<IResult> ListAsync(
        Guid medicinalProductId,
        ListDrugInteractionsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListDrugInteractionsQuery(
                new MedicinalProductId(medicinalProductId)),
            cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> RecordAsync(
        Guid medicinalProductId,
        RecordInteractionRequest request,
        RecordDrugInteractionHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new RecordDrugInteractionCommand(
                new MedicinalProductId(medicinalProductId),
                request.InteractionTypeCode,
                request.LabelText,
                request.Interactant,
                request.InteractantSubstanceId is { } id
                    ? new SubstanceId(id)
                    : null,
                request.Management,
                request.SeverityCode),
            cancellationToken);

        return Results.Created(
            $"/api/interactions/{result.Id.Value}",
            new ClinicalStatementResponse(result.Id.Value));
    }

    private static async Task<IResult> AddInteractantAsync(
        Guid interactionId,
        InteractantRequest request,
        AddInteractantHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new AddInteractantCommand(
                DrugInteractionId.From(interactionId),
                request.Description,
                request.SubstanceId is { } id ? new SubstanceId(id) : null),
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> RemoveInteractantAsync(
        Guid interactionId,
        Guid interactantId,
        RemoveInteractantHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RemoveInteractantCommand(
                DrugInteractionId.From(interactionId),
                InteractantId.From(interactantId)),
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> AddAsync(
        Guid interactionId,
        StatementPopulationRequest request,
        AddDrugInteractionPopulationHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new AddDrugInteractionPopulationCommand(
                DrugInteractionId.From(interactionId),
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
        Guid interactionId,
        Guid populationId,
        StatementPopulationRequest request,
        AmendDrugInteractionPopulationHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new AmendDrugInteractionPopulationCommand(
                DrugInteractionId.From(interactionId),
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
        Guid interactionId,
        Guid populationId,
        RemoveDrugInteractionPopulationHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RemoveDrugInteractionPopulationCommand(
                DrugInteractionId.From(interactionId),
                PopulationId.From(populationId)),
            cancellationToken);

        return Results.NoContent();
    }
}
