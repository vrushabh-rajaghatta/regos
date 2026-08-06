using Microsoft.EntityFrameworkCore;

using RegOS.Process.Domain.Aggregates.ProcessDefinitions;

namespace RegOS.Persistence.Initialization.Process;

/// <summary>
/// Brings a database's playbooks up to what the seed describes — additively, and
/// one version at a time.
/// </summary>
/// <remarks>
/// <b>Idempotent per version, not per definition</b> — the shape
/// <c>RegulatoryTemplateDataInitializer</c> arrived at, and for the same reason.
/// Skipping any definition whose id is already present would make the seed
/// authoritative only for databases that had never seen it, so a playbook could
/// never be corrected after its first insert.
/// <para>
/// And a correction here is always a <em>new version</em>, never an edit: a
/// published <see cref="ProcessDefinitionVersion"/> is frozen (ADR-065 I4)
/// because a plan may already be pinned to it. This is what carries the new
/// version into databases that already hold the old one, from the same code a
/// clean clone runs.
/// </para>
/// </remarks>
public sealed class ProcessDefinitionDataInitializer : IDataInitializer
{
    private readonly RegOSDbContext _dbContext;

    public ProcessDefinitionDataInitializer(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        var seeded = ProcessDefinitions.Data;
        var seededIds = seeded.Select(x => x.Id).ToList();

        // IgnoreQueryFilters: startup has no tenant, and playbooks carry the
        // shared-plus-tenant filter (ADR-031) — without this the filter would
        // report an empty table and re-insert on every boot.
        var live = await _dbContext.ProcessDefinitions
            .IgnoreQueryFilters()
            .Include(x => x.Versions)
            .Where(x => seededIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        var liveById = live.ToDictionary(x => x.Id);

        foreach (var definition in seeded)
        {
            if (!liveById.TryGetValue(definition.Id, out var current))
            {
                // Never seen here: insert the playbook whole, every version.
                _dbContext.ProcessDefinitions.Add(definition);
                continue;
            }

            AddMissingVersions(definition, current);
            SupersedeRetiredVersions(definition, current);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Inserts versions the seed has and this database does not.
    /// </summary>
    /// <remarks>
    /// Matched by version <b>number</b>, never by id: step and version ids are
    /// generated, so the same playbook carries different ids in every database.
    /// The number is the only identity a seed and a database share.
    /// </remarks>
    private void AddMissingVersions(
        ProcessDefinition seeded,
        ProcessDefinition current)
    {
        var present = current.Versions
            .Select(x => x.VersionNumber)
            .ToHashSet();

        foreach (var version in seeded.Versions)
        {
            if (present.Contains(version.VersionNumber))
                continue;

            // Add() walks the graph, so the version's steps and their
            // predecessors come with it. The parent link is a shadow FK and has
            // to be set by hand — the seed's graph is detached, and the
            // definition it belongs to is not the instance being tracked here.
            _dbContext.Add(version);
            _dbContext.Entry(version)
                .Property("ProcessDefinitionId")
                .CurrentValue = seeded.Id;
        }
    }

    /// <summary>
    /// Applies <c>Published → Superseded</c> where the seed has since retired a
    /// version this database already holds.
    /// </summary>
    /// <remarks>
    /// The only status change applied to existing rows, and it is safe precisely
    /// because it changes nothing a pinned plan depends on: the steps, offsets
    /// and predecessors stay exactly as they were. What it removes is eligibility
    /// for <em>new</em> instantiation — which is the whole meaning of the status.
    /// <para>
    /// It goes through the aggregate rather than writing the column, so the same
    /// rules apply here as anywhere: a draft cannot be superseded, and
    /// superseding twice is an error.
    /// </para>
    /// </remarks>
    private static void SupersedeRetiredVersions(
        ProcessDefinition seeded,
        ProcessDefinition current)
    {
        var supersededNumbers = seeded.Versions
            .Where(x => x.Status == ProcessDefinitionVersionStatus.Superseded)
            .Select(x => x.VersionNumber)
            .ToHashSet();

        foreach (var version in current.Versions)
        {
            if (!supersededNumbers.Contains(version.VersionNumber))
                continue;

            if (version.Status != ProcessDefinitionVersionStatus.Published)
                continue;

            current.SupersedeVersion(version.Id);
        }
    }
}
