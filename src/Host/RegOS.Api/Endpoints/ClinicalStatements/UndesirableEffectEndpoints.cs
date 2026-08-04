using RegOS.Labeling.Application.Commands.AddUndesirableEffectPopulation;
using RegOS.Labeling.Application.Commands.AmendUndesirableEffectPopulation;
using RegOS.Labeling.Application.Commands.RecordUndesirableEffect;
using RegOS.Labeling.Application.Commands.RemoveUndesirableEffectPopulation;
using RegOS.Labeling.Application.Commands.RestateUndesirableEffectText;
using RegOS.Labeling.Application.Queries.ListUndesirableEffects;
using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;
using RegOS.Labeling.Domain.Aggregates.UndesirableEffects;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.ClinicalStatements;

public sealed record RecordUndesirableEffectRequest(
    string ConditionCode,
    string LabelText,
    string? FrequencyCode);

/// <summary>
/// One statement type, five routes — recorded, reworded, and the three
/// population operations.
/// </summary>
/// <remarks>
/// <b>No decision route</b>, unlike an indication: this is content inside an
/// approved label, so what changes it is a new label revision rather than a
/// decision recorded here (EPIC-018 S004).
/// </remarks>
public static class UndesirableEffectEndpoints
{
    public static IEndpointRouteBuilder MapUndesirableEffects(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/medicinal-products/{medicinalProductId:guid}/undesirable-effects", ListAsync);

        app.MapPost(
            "/api/medicinal-products/{medicinalProductId:guid}/undesirable-effects", RecordAsync);

        app.MapPut("/api/undesirable-effects/{statementId:guid}/text", RestateAsync);

        app.MapPost("/api/undesirable-effects/{statementId:guid}/populations", AddAsync);

        app.MapPut(
            "/api/undesirable-effects/{statementId:guid}/populations/{populationId:guid}",
            AmendAsync);

        app.MapDelete(
            "/api/undesirable-effects/{statementId:guid}/populations/{populationId:guid}",
            RemoveAsync);

        return app;
    }

    private static async Task<IResult> ListAsync(
        Guid medicinalProductId,
        ListUndesirableEffectsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListUndesirableEffectsQuery(new MedicinalProductId(medicinalProductId)),
            cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> RecordAsync(
        Guid medicinalProductId,
        RecordUndesirableEffectRequest request,
        RecordUndesirableEffectHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new RecordUndesirableEffectCommand(
                new MedicinalProductId(medicinalProductId),
                request.ConditionCode,
                request.LabelText,
                request.FrequencyCode),
            cancellationToken);

        return Results.Created(
            $"/api/undesirable-effects/{result.Id.Value}",
            new ClinicalStatementResponse(result.Id.Value));
    }

    private static async Task<IResult> RestateAsync(
        Guid statementId,
        RestateStatementTextRequest request,
        RestateUndesirableEffectTextHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RestateUndesirableEffectTextCommand(
                UndesirableEffectId.From(statementId), request.LabelText),
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> AddAsync(
        Guid statementId,
        StatementPopulationRequest request,
        AddUndesirableEffectPopulationHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new AddUndesirableEffectPopulationCommand(
                UndesirableEffectId.From(statementId),
                request.AgeLow,
                request.AgeHigh,
                request.AgeUnitCode,
                request.GenderCode,
                request.PhysiologicalConditionCode,
                request.Description),
            cancellationToken);

        return Results.NoContent();
    }

    /// <remarks>
    /// A PUT on the population's own id — the qualifier keeps its identity
    /// through a correction. Second and third demonstrations of EPIC-018 D2.
    /// </remarks>
    private static async Task<IResult> AmendAsync(
        Guid statementId,
        Guid populationId,
        StatementPopulationRequest request,
        AmendUndesirableEffectPopulationHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new AmendUndesirableEffectPopulationCommand(
                UndesirableEffectId.From(statementId),
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
        Guid statementId,
        Guid populationId,
        RemoveUndesirableEffectPopulationHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RemoveUndesirableEffectPopulationCommand(
                UndesirableEffectId.From(statementId),
                PopulationId.From(populationId)),
            cancellationToken);

        return Results.NoContent();
    }
}
