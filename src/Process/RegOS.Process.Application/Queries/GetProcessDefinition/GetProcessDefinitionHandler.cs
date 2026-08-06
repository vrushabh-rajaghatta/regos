using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Process.Domain.Aggregates.ProcessDefinitions;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Process.Application.Queries.GetProcessDefinition;

/// <summary>
/// One playbook, read whole.
/// </summary>
/// <remarks>
/// <b>Composed in memory from one round trip rather than assembled in SQL.</b>
/// A playbook is a handful of versions of a dozen steps each — a team's authored
/// process, not a warehouse — and the predecessor codes need the step set anyway
/// to resolve an id to a code. A readable projection is worth more here than a
/// clever one (the argument <c>ListDueWork</c> already made).
/// </remarks>
public sealed class GetProcessDefinitionHandler
{
    private readonly RegOSDbContext _dbContext;

    public GetProcessDefinitionHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProcessDefinitionDetails> HandleAsync(
        GetProcessDefinitionQuery query,
        CancellationToken cancellationToken = default)
    {
        var id = ProcessDefinitionId.From(query.Id);

        var row = await (
            from definition in _dbContext.ProcessDefinitions.AsNoTracking()
            where definition.Id == id
            join country in _dbContext.Countries
                on definition.CountryId equals country.Id
            join authority in _dbContext.Authorities
                on definition.AuthorityId equals authority.Id
            join applicationType in _dbContext.ApplicationTypes
                on definition.ApplicationTypeId equals applicationType.Id
            select new
            {
                definition.Id,
                definition.Code,
                definition.Name,
                definition.Description,
                IsShared = definition.TenantId == null,
                CountryCode = country.Code,
                CountryName = country.Name,
                AuthorityCode = authority.Code,
                AuthorityName = authority.Name,
                ApplicationTypeCode = applicationType.Code,
                ApplicationTypeName = applicationType.Name,
                definition.Status,
                definition.CreatedOnUtc,
                Versions = definition.Versions
                    .Select(version => new
                    {
                        version.Id,
                        version.VersionNumber,
                        version.Status,
                        version.EffectiveFrom,
                        version.PublishedOnUtc,
                        Steps = version.Steps
                            .Select(step => new
                            {
                                step.Id,
                                step.Code,
                                step.Name,
                                step.Description,
                                step.ParentStepId,
                                step.Order,
                                step.OffsetDays,
                                step.DurationDays,
                                Predecessors = step.Predecessors
                                    .Select(x => x.PredecessorStepId)
                                    .ToList()
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("That playbook does not exist.");

        // Newest first — how a version history is read.
        // Deterministic: version numbers are unique per playbook (unique index
        // on (ProcessDefinitionId, VersionNumber)), and every version here
        // belongs to one playbook.
        var ordered = row.Versions
            .OrderByDescending(x => x.VersionNumber)
            .ToList();

        // EffectiveTo is the day before the next version began — derived here so
        // that no column can disagree with the version that owns it.
        var effectiveTo = new Dictionary<int, DateOnly?>();

        foreach (var version in ordered)
        {
            var successor = ordered
                .Where(x => x.VersionNumber > version.VersionNumber)
                .Where(x => x.EffectiveFrom is not null)
                // Deterministic: version numbers are unique per playbook, so a
                // single minimum exists whenever this is non-empty.
                .OrderBy(x => x.VersionNumber)
                .FirstOrDefault();

            effectiveTo[version.VersionNumber] =
                successor?.EffectiveFrom?.AddDays(-1);
        }

        var selected = ordered
                .FirstOrDefault(x => x.VersionNumber == query.VersionNumber)
            ?? ordered.FirstOrDefault(
                x => x.Status == ProcessDefinitionVersionStatus.Published)
            ?? ordered.FirstOrDefault();

        var codeOf = selected?.Steps.ToDictionary(x => x.Id, x => x.Code)
                     ?? [];

        return new ProcessDefinitionDetails(
            row.Id.Value,
            row.Code,
            row.Name,
            row.Description,
            row.IsShared,
            row.CountryCode,
            row.CountryName,
            row.AuthorityCode,
            row.AuthorityName,
            row.ApplicationTypeCode,
            row.ApplicationTypeName,
            row.Status.ToString(),
            row.CreatedOnUtc,
            [.. ordered.Select(version => new ProcessDefinitionVersionSummary(
                version.Id.Value,
                version.VersionNumber,
                version.Status.ToString(),
                version.EffectiveFrom,
                effectiveTo[version.VersionNumber],
                version.PublishedOnUtc,
                version.Steps.Count))],
            selected?.VersionNumber,
            selected is null
                ? []
                : [.. selected.Steps
                    // Authored order, then code. Deterministic: Order is
                    // deliberately not unique — two steps may sit side by side —
                    // and step code carries a unique index per version, so the
                    // pair is total.
                    .OrderBy(step => step.Order)
                    .ThenBy(step => step.Code, StringComparer.Ordinal)
                    .Select(step => new ProcessStepDetails(
                        step.Id.Value,
                        step.Code,
                        step.Name,
                        step.Description,
                        step.ParentStepId?.Value,
                        step.Order,
                        step.OffsetDays,
                        step.DurationDays,
                        [.. step.Predecessors
                            .Select(x => codeOf.GetValueOrDefault(x, "?"))
                            // Deterministic: predecessor codes are distinct
                            // within a step — a unique index on
                            // (StepId, PredecessorStepId) and one code per step.
                            .OrderBy(code => code, StringComparer.Ordinal)]))]);
    }
}
