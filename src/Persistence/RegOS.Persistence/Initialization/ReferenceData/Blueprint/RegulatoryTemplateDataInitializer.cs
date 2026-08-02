using Microsoft.EntityFrameworkCore;

using RegOS.ReferenceData.Domain.Blueprint;

namespace RegOS.Persistence.Initialization.ReferenceData.Blueprint;

/// <summary>
/// Brings a database's blueprints up to what the seed describes — additively,
/// and one version at a time.
/// </summary>
/// <remarks>
/// <b>Idempotent per version, not per template.</b> It used to skip any
/// template whose id was already present, which meant a blueprint could never
/// be corrected after its first insert: the seed was authoritative only for
/// databases that had never seen it.
/// <para>
/// That mattered the first time a published version turned out to be wrong
/// (EPIC-007a S002). A published version is frozen, so the correction is a
/// <em>new</em> version — and this is what carries it into databases that
/// already hold the old one, from the same code a clean clone runs. One source
/// of truth, so the two cannot drift.
/// </para>
/// </remarks>
public sealed class RegulatoryTemplateDataInitializer : IDataInitializer
{
    private readonly RegOSDbContext _dbContext;

    public RegulatoryTemplateDataInitializer(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        var seeded = RegulatoryTemplates.Data;
        var seededIds = seeded.Select(x => x.Id).ToList();

        // IgnoreQueryFilters: startup has no tenant, and templates carry the
        // shared-plus-tenant filter (ADR-031) — without this the filter would
        // report an empty table and re-insert on every boot.
        var live = await _dbContext.RegulatoryTemplates
            .IgnoreQueryFilters()
            .Include(x => x.Versions)
            .Where(x => seededIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        var liveById = live.ToDictionary(x => x.Id);

        foreach (var template in seeded)
        {
            if (!liveById.TryGetValue(template.Id, out var current))
            {
                // Never seen here: insert the blueprint whole, every version.
                _dbContext.RegulatoryTemplates.Add(template);
                continue;
            }

            AddMissingVersions(template, current);
            DeprecateSupersededVersions(template, current);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Inserts versions the seed has and this database does not.
    /// </summary>
    /// <remarks>
    /// Matched by version <b>number</b>, never by id: section and version ids
    /// are generated (<c>TemplateSectionId.New()</c>), so the same blueprint
    /// carries different ids in every database. The number is the only identity
    /// a seed and a database share.
    /// </remarks>
    private void AddMissingVersions(
        RegulatoryTemplate seeded,
        RegulatoryTemplate current)
    {
        var present = current.Versions
            .Select(v => v.VersionNumber)
            .ToHashSet();

        foreach (var version in seeded.Versions)
        {
            if (present.Contains(version.VersionNumber))
                continue;

            // Add() walks the graph, so the version's sections, required
            // documents and rules come with it. The parent link is a shadow FK
            // and has to be set by hand — the seed's graph is detached, and the
            // template it belongs to is not the instance being tracked here.
            _dbContext.Add(version);
            _dbContext.Entry(version)
                .Property("RegulatoryTemplateId")
                .CurrentValue = seeded.Id;
        }
    }

    /// <summary>
    /// Applies <c>Published → Deprecated</c> where the seed has since
    /// superseded a version this database already holds.
    /// </summary>
    /// <remarks>
    /// This is the only status change applied to existing rows, and it is safe
    /// precisely because it changes nothing a bound submission depends on: the
    /// sections, required documents and rules stay exactly as they were. What
    /// it removes is eligibility for <em>new</em> bindings.
    /// <para>
    /// It goes through <see cref="RegulatoryTemplate.DeprecateVersion"/> rather
    /// than writing the column, so the same rules apply here as anywhere —
    /// a draft cannot be deprecated, and deprecating twice is an error.
    /// </para>
    /// </remarks>
    private static void DeprecateSupersededVersions(
        RegulatoryTemplate seeded,
        RegulatoryTemplate current)
    {
        var supersededNumbers = seeded.Versions
            .Where(v => v.Status == TemplateVersionStatus.Deprecated)
            .Select(v => v.VersionNumber)
            .ToHashSet();

        var stale = current.Versions
            .Where(v => v.Status == TemplateVersionStatus.Published
                && supersededNumbers.Contains(v.VersionNumber))
            .ToList();

        foreach (var version in stale)
            current.DeprecateVersion(version.Id);
    }
}
