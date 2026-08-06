using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Process.Domain.Aggregates.ProcessDefinitions;
using RegOS.Process.Domain.Aggregates.ProcessObjectives;
using RegOS.Process.Domain.Aggregates.ProcessPlans;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Process.Application.Commands.InstantiateProcessPlan;

/// <summary>
/// Loads the two things a plan is made from and hands them to the aggregate.
/// </summary>
/// <remarks>
/// <b>The handler resolves; the aggregate decides.</b> Every rule that matters —
/// the version must be published, the schedule is derived once, the graph is
/// translated into the plan's own ids — lives in
/// <c>ProcessPlan.InstantiateFrom</c>, which is a pure function of its arguments
/// and needs no database at all (ADR-065 I5).
/// <para>
/// <b>The definition is loaded whole, through its repository.</b> A read model
/// would be cheaper and would be wrong: the derivation needs every step's offset,
/// duration and predecessor set, and a projection that dropped one would produce
/// a schedule that looked right.
/// </para>
/// </remarks>
public sealed class InstantiateProcessPlanHandler
{
    private readonly IProcessPlanRepository _plans;
    private readonly IProcessDefinitionRepository _definitions;
    private readonly RegOSDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public InstantiateProcessPlanHandler(
        IProcessPlanRepository plans,
        IProcessDefinitionRepository definitions,
        RegOSDbContext dbContext,
        ITenantContext tenantContext)
    {
        _plans = plans;
        _definitions = definitions;
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<InstantiateProcessPlanResult> HandleAsync(
        InstantiateProcessPlanCommand command,
        CancellationToken cancellationToken)
    {
        // Fail-closed by the query filter: another tenant's objective is
        // indistinguishable from one that does not exist (ADR-031).
        var objectiveExists = await _dbContext.ProcessObjectives
            .AsNoTracking()
            .AnyAsync(x => x.Id == command.ProcessObjectiveId, cancellationToken);

        if (!objectiveExists)
            throw new NotFoundException(ProcessPlanErrors.ObjectiveRequired);

        // Which playbook owns this version is not known to the caller, so the
        // version is found by walking the definitions the tenant can see. The
        // shared-plus-extensible filter decides that set.
        var definitionId = await _dbContext.ProcessDefinitions
            .AsNoTracking()
            .Where(x => x.Versions.Any(
                v => v.Id == command.ProcessDefinitionVersionId))
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(
                "That playbook version does not exist.");

        var definition =
            await _definitions.GetByIdAsync(definitionId, cancellationToken)
            ?? throw new NotFoundException("That playbook does not exist.");

        var version = definition.Versions
            .Single(x => x.Id == command.ProcessDefinitionVersionId);

        var plan = ProcessPlan.InstantiateFrom(
            _tenantContext.TenantId,
            command.ProcessObjectiveId,
            version,
            command.AnchorDate,
            command.Name,
            command.OpenedOn);

        await _plans.AddAsync(plan, cancellationToken);

        return new InstantiateProcessPlanResult(plan.Id.Value, plan.Steps.Count);
    }
}
