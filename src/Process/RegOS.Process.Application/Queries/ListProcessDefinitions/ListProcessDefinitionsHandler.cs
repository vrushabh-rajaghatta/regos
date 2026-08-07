using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Process.Domain.Aggregates.ProcessDefinitions;

namespace RegOS.Process.Application.Queries.ListProcessDefinitions;

/// <summary>
/// The playbook index — <em>"what do we have to do to file this, and in what
/// order?"</em>, answered at the level of which playbooks exist.
/// </summary>
/// <remarks>
/// Reads compose over <c>RegOSDbContext</c> with <c>AsNoTracking()</c>
/// (ADR-016); the tenant filter decides visibility, not this handler (ADR-031).
/// The shared set and the tenant's own arrive together and are distinguished by
/// <see cref="ProcessDefinitionSummary.IsShared"/> rather than by two queries.
/// </remarks>
public sealed class ListProcessDefinitionsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListProcessDefinitionsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProcessDefinitionSummary>> HandleAsync(
        ListProcessDefinitionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var definitions = _dbContext.ProcessDefinitions.AsNoTracking();

        if (!query.IncludeRetired)
        {
            definitions = definitions.Where(
                x => x.Status == ProcessDefinitionStatus.Active);
        }

        var rows = await (
            from definition in definitions
            join country in _dbContext.Countries
                on definition.CountryId equals country.Id
            join authority in _dbContext.Authorities
                on definition.AuthorityId equals authority.Id
            join applicationType in _dbContext.ApplicationTypes
                on definition.ApplicationTypeId equals applicationType.Id
            orderby country.Code, definition.Name, definition.Id
            select new ProcessDefinitionSummary(
                definition.Id.Value,
                definition.Code,
                definition.Name,
                definition.Description,
                definition.TenantId == null,
                country.Code,
                country.Name,
                authority.Name,
                applicationType.Name,
                definition.Status.ToString(),
                definition.Versions
                    .Where(v => v.Status == ProcessDefinitionVersionStatus.Published)
                    .Select(v => (int?)v.VersionNumber)
                    .Max(),
                definition.Versions.Count,
                definition.Versions.Any(
                    v => v.Status == ProcessDefinitionVersionStatus.Draft)))
            .ToListAsync(cancellationToken);

        return rows;
    }
}
