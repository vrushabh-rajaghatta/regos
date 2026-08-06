using RegOS.Process.Application.Commands.InstantiateProcessPlan;
using RegOS.Process.Domain.Aggregates.ProcessDefinitions;
using RegOS.Process.Domain.Aggregates.ProcessObjectives;

namespace RegOS.Api.Endpoints.ProcessPlans;

public static class InstantiateProcessPlanEndpoint
{
    public static IEndpointRouteBuilder MapInstantiateProcessPlanEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/process-plans", HandleAsync);

        return endpoints;
    }

    /// <summary>
    /// The request names a playbook <em>version</em>, not a playbook. Resolving
    /// "the current one" server-side would make a plan's schedule depend on when
    /// it was created rather than on what it was created from (ADR-065 I5).
    /// </summary>
    private static async Task<IResult> HandleAsync(
        InstantiateProcessPlanRequest request,
        InstantiateProcessPlanHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new InstantiateProcessPlanCommand(
                ProcessObjectiveId.From(request.ProcessObjectiveId),
                ProcessDefinitionVersionId.From(request.ProcessDefinitionVersionId),
                request.AnchorDate,
                request.Name,
                request.OpenedOn ?? request.AnchorDate),
            cancellationToken);

        return Results.Created($"/api/process-plans/{result.Id}", result);
    }

    public sealed record InstantiateProcessPlanRequest(
        Guid ProcessObjectiveId,
        Guid ProcessDefinitionVersionId,
        DateOnly AnchorDate,
        string Name,
        DateOnly? OpenedOn);
}
