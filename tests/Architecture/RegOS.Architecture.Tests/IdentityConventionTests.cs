using System.Text.RegularExpressions;
using FluentAssertions;

namespace RegOS.Architecture.Tests;

/// <summary>
/// ES-020 — an entity identity derives from <c>StronglyTypedId</c>.
///
/// The older form, <c>readonly record struct XId(Guid Value)</c>, is strongly
/// typed and still fails this rule. That is the whole reason the rule needed a
/// test: for most of this project the convention was written as "strongly
/// typed, never a bare Guid", which a record struct satisfies, so nothing ever
/// flagged it.
///
/// What the older form actually costs is not the id but the entity.
/// <c>Entity&lt;TId&gt;</c> constrains <c>TId : StronglyTypedId</c>, so an
/// entity keyed by a record struct cannot inherit <c>Entity&lt;TId&gt;</c> or
/// <c>AggregateRoot&lt;TId&gt;</c> at all. It has no base class, no identity
/// equality, and no empty-guid guard — an all-zero id in a route becomes a 500
/// instead of a 400 (ADR-017).
///
/// ADR-017 recorded a policy of migrating these "one bounded context per
/// story". Over the life of that policy the count went up, not down: every
/// status-history entry added since took the record-struct form, copied from
/// the previous one. That is what this test exists to stop. The migration is
/// tracked by <see cref="PendingMigration"/>; the rule holds for everything
/// written from here.
/// </summary>
public sealed class IdentityConventionTests
{
    /// <summary>
    /// Identities that predate ES-020 and are still to be migrated. Shrink,
    /// never grow.
    ///
    /// Migrating one is mechanical — the id becomes a sealed class deriving
    /// from StronglyTypedId, the entity gains an AggregateRoot/Entity base, and
    /// the EF value converter changes shape — but it is wide, so it is done a
    /// whole bounded context at a time, when that context is being worked on
    /// for another reason. Half-migrated contexts are worse than either end
    /// state.
    /// </summary>
    private static readonly HashSet<string> PendingMigration =
    [
        // ReferenceData · Blueprint — the metadata engine. RegulatoryTemplate
        // is an aggregate root that owns its versions; these are not lookups.
        "RegulatoryTemplateId",
        "RegulatoryTemplateVersionId",
        "TemplateSectionId",
        "RequiredDocumentId",
        "ValidationRuleId",

        // Registration
        "RegistrationId",
        "RegistrationStatusEntryId",

        // ProductDocument
        "ProductDocumentId",
        "DocumentVersionId",

        // RegulatoryApplication
        "RegulatoryApplicationId",

        // Status-history entries in contexts whose aggregates already conform.
        // These are the regressions: each was copied from the last.
        "MarketStatusEntryId",
        "CommitmentStatusEntryId",
        "InspectionStatusEntryId",
        "HaMeetingStatusEntryId",
        "HaQuestionStatusEntryId"
    ];

    /// <summary>
    /// Flat master-data lookups, deliberately outside ES-020 — not a backlog.
    ///
    /// These are the ES-015 records: platform-assigned deterministic ids, no
    /// child entities, no lifecycle beyond Create, never loaded as an aggregate
    /// to be mutated. They are closer to value objects than to entities, and
    /// nothing here inherits Entity&lt;TId&gt; or wants to, so the base-class
    /// argument that justifies the rule does not apply to them.
    ///
    /// This list grows only with an ADR. Adding a type here to make new code
    /// pass is the failure mode the shrink-only mechanism exists to prevent —
    /// if a lookup grows children or a lifecycle, it has stopped being master
    /// data and moves to <see cref="PendingMigration"/> instead.
    /// </summary>
    private static readonly HashSet<string> MasterDataLookups =
    [
        "CountryId",
        "DocumentTypeId",
        "SubmissionTypeId",
        "AuthorityId",
        "AuthorityDivisionId",
        "CorrespondenceTypeId",
        "ContactRoleId",
        "IdentifierSchemeId"
    ];

    [Fact]
    public void Every_entity_identity_derives_from_StronglyTypedId()
    {
        var offenders = RecordStructIdentities()
            .Where(id => !PendingMigration.Contains(id.Name))
            .Where(id => !MasterDataLookups.Contains(id.Name))
            .Select(id => $"{id.Name} ({id.Path})")
            .ToList();

        offenders.Should().BeEmpty(
            "an entity identity is a sealed class deriving from "
            + "StronglyTypedId (ENGINEERING_STANDARDS.md ES-020) — copy "
            + "src/Interaction/RegOS.Interaction.Domain/Commitments/CommitmentId.cs. "
            + "A readonly record struct is strongly typed but cannot satisfy "
            + "the Entity<TId> constraint, so the entity silently loses its "
            + "base class, identity equality and empty-guid guard");
    }

    [Fact]
    public void The_pending_migration_list_contains_no_stale_entries()
    {
        var outstanding = RecordStructIdentities().Select(id => id.Name).ToHashSet();

        var stale = PendingMigration.Where(name => !outstanding.Contains(name)).ToList();

        stale.Should().BeEmpty(
            "these identities now derive from StronglyTypedId, or no longer "
            + "exist — delete them from IdentityConventionTests.PendingMigration");
    }

    [Fact]
    public void The_master_data_exclusion_contains_no_stale_entries()
    {
        var outstanding = RecordStructIdentities().Select(id => id.Name).ToHashSet();

        var stale = MasterDataLookups.Where(name => !outstanding.Contains(name)).ToList();

        stale.Should().BeEmpty(
            "these lookups no longer use a record struct id — delete them from "
            + "IdentityConventionTests.MasterDataLookups so the exclusion does "
            + "not outlive the thing it excused");
    }

    private static readonly Regex RecordStructId = new(
        @"readonly\s+record\s+struct\s+(?<name>\w+Id)\s*\(\s*Guid\s+Value\s*\)",
        RegexOptions.Compiled);

    /// <summary>
    /// Every <c>readonly record struct XId(Guid Value)</c> declared under src/.
    ///
    /// Keyed by type name rather than by path, unlike the other convention
    /// tests: several of these are declared at the foot of the entity file
    /// rather than in a file of their own (every *StatusEntryId is), so a path
    /// does not identify one. Type names are unique across the solution.
    /// </summary>
    private static IEnumerable<(string Name, string Path)> RecordStructIdentities() =>
        Repo.SourceFiles("src")
            .SelectMany(file => RecordStructId
                .Matches(Repo.CodeOf(file))
                .Select(match => (match.Groups["name"].Value, Repo.Relative(file))))
            .OrderBy(id => id.Value, StringComparer.Ordinal);
}
