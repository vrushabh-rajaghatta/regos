using RegOS.Labeling.Application.Commands.AddContraindicationPopulation;
using RegOS.Labeling.Application.Commands.AmendContraindicationPopulation;
using RegOS.Labeling.Application.Commands.RecordContraindication;
using RegOS.Labeling.Application.Commands.RemoveContraindicationPopulation;
using RegOS.Labeling.Application.Commands.RestateContraindicationText;
using RegOS.Labeling.Application.Queries.ListContraindications;
using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;
using RegOS.Labeling.Domain.Aggregates.Contraindications;
using RegOS.Product.Domain.Product;

namespace RegOS.Api.Endpoints.ClinicalStatements;

public sealed record RecordContraindicationRequest(
    string ConditionCode,
    string LabelText);

/// <summary>
/// One statement type, five routes — recorded, reworded, and the three
/// population operations.
/// </summary>
/// <remarks>
/// <b>No decision route</b>, unlike an indication: this is content inside an
/// approved label, so what changes it is a new label revision rather than a
/// decision recorded here (EPIC-018 S004).
/// </remarks>
public static class ContraindicationEndpoints
{
    public static IEndpointRouteBuilder MapContraindications(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/medicinal-products/{medicinalProductId:guid}/contraindications", ListAsync);

        app.MapPost(
            "/api/medicinal-products/{medicinalProductId:guid}/contraindications", RecordAsync);

        app.MapPut("/api/contraindications/{statementId:guid}/text", RestateAsync);

        app.MapPost("/api/contraindications/{statementId:guid}/populations", AddAsync);

        app.MapPut(
            "/api/contraindications/{statementId:guid}/populations/{populationId:guid}",
            AmendAsync);

        app.MapDelete(
            "/api/contraindications/{statementId:guid}/populations/{populationId:guid}",
            RemoveAsync);

        return app;
    }

    private static async Task<IResult> ListAsync(
        Guid medicinalProductId,
        ListContraindicationsHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new ListContraindicationsQuery(new MedicinalProductId(medicinalProductId)),
            cancellationToken);

        return Results.Ok(result);
    }

    private static async Task<IResult> RecordAsync(
        Guid medicinalProductId,
        RecordContraindicationRequest request,
        RecordContraindicationHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new RecordContraindicationCommand(
                new MedicinalProductId(medicinalProductId),
                request.ConditionCode,
                request.LabelText),
            cancellationToken);

        return Results.Created(
            $"/api/contraindications/{result.Id.Value}",
            new ClinicalStatementResponse(result.Id.Value));
    }

    private static async Task<IResult> RestateAsync(
        Guid statementId,
        RestateStatementTextRequest request,
        RestateContraindicationTextHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RestateContraindicationTextCommand(
                ContraindicationId.From(statementId), request.LabelText),
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> AddAsync(
        Guid statementId,
        StatementPopulationRequest request,
        AddContraindicationPopulationHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new AddContraindicationPopulationCommand(
                ContraindicationId.From(statementId),
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
        AmendContraindicationPopulationHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new AmendContraindicationPopulationCommand(
                ContraindicationId.From(statementId),
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
        RemoveContraindicationPopulationHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(
            new RemoveContraindicationPopulationCommand(
                ContraindicationId.From(statementId),
                PopulationId.From(populationId)),
            cancellationToken);

        return Results.NoContent();
    }
}
