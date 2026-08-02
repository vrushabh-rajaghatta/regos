using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RegOS.Persistence.Configurations;

/// <summary>
/// The one mapping shared by every owned append-only status history in RegOS.
/// </summary>
/// <remarks>
/// <para>
/// <b>This is the extraction ADR-042 licensed and ADR-046 decision 6 called
/// for</b>, and its scope is deliberately narrow: <em>the EF configuration
/// only</em>. Not the entry types, and not the behaviour. ADR-042's finding
/// that <em>structural similarity is not evidence of behavioural similarity</em>
/// is unchanged — seven histories still mean seven different domains' rules,
/// enforced by seven different sets of tests. What they genuinely share is how
/// five columns land in a table, which is all this maps.
/// </para>
/// <para>
/// ADR-042 set the bar at five owned configurations and named the measurement
/// that would meet it. <c>SubmissionStatusEntry</c> (EPIC-004 S003) was the
/// fifth, and four of the five were line-for-line identical. That is the
/// evidence; the rule of three (ADR-018) is satisfied by demonstration, not by
/// symmetry.
/// </para>
/// <para>
/// <b>Why the property names are strings.</b> Every entry type names its five
/// members identically — <c>Id</c>, <c>Status</c>, <c>OccurredOn</c>,
/// <c>RecordedOnUtc</c>, <c>Note</c> — but they share no interface, because
/// giving them one would mean changing the entry types, which is exactly what
/// this extraction refuses to do. The alternative is an expression selector per
/// property, which costs more at each call site than the block it replaces. So
/// the names are matched by string, and a rename that breaks one fails loudly
/// when the model is built — which every integration test does before it runs
/// its first assertion.
/// </para>
/// <para>
/// The table and foreign-key names stay explicit at the call site on purpose.
/// Both are database facts that appear in migrations; deriving them from a type
/// name would let a domain rename silently rename a column.
/// </para>
/// </remarks>
internal static class StatusHistoryMapping
{
    private const string Id = "Id";
    private const string Status = "Status";
    private const string OccurredOn = "OccurredOn";
    private const string RecordedOnUtc = "RecordedOnUtc";
    private const string Note = "Note";

    /// <summary>
    /// Maps an append-only status history owned directly by an aggregate root —
    /// commitment, meeting, inspection, submission.
    /// </summary>
    public static EntityTypeBuilder<TOwner> OwnsStatusHistory<TOwner, TEntry, TEntryId>(
        this EntityTypeBuilder<TOwner> builder,
        Expression<Func<TOwner, IEnumerable<TEntry>?>> history,
        string tableName,
        string ownerForeignKey,
        Expression<Func<TEntryId, Guid>> toGuid,
        Expression<Func<Guid, TEntryId>> fromGuid,
        int noteMaxLength)
        where TOwner : class
        where TEntry : class
    {
        builder.OwnsMany(
            history,
            entry => Configure(entry, tableName, ownerForeignKey, toGuid, fromGuid, noteMaxLength));

        builder.Navigation(history).AutoInclude();

        return builder;
    }

    /// <summary>
    /// Maps a status history owned by an entity that is itself owned — today
    /// only <c>HaQuestion</c>, inside <c>HaCorrespondence</c>.
    /// </summary>
    public static OwnedNavigationBuilder<TParent, TOwner> OwnsStatusHistory<TParent, TOwner, TEntry, TEntryId>(
        this OwnedNavigationBuilder<TParent, TOwner> builder,
        Expression<Func<TOwner, IEnumerable<TEntry>?>> history,
        string tableName,
        string ownerForeignKey,
        Expression<Func<TEntryId, Guid>> toGuid,
        Expression<Func<Guid, TEntryId>> fromGuid,
        int noteMaxLength)
        where TParent : class
        where TOwner : class
        where TEntry : class
    {
        builder.OwnsMany(
            history,
            entry => Configure(entry, tableName, ownerForeignKey, toGuid, fromGuid, noteMaxLength));

        builder.Navigation(history).AutoInclude();

        return builder;
    }

    private static void Configure<TOwner, TEntry, TEntryId>(
        OwnedNavigationBuilder<TOwner, TEntry> entry,
        string tableName,
        string ownerForeignKey,
        Expression<Func<TEntryId, Guid>> toGuid,
        Expression<Func<Guid, TEntryId>> fromGuid,
        int noteMaxLength)
        where TOwner : class
        where TEntry : class
    {
        entry.ToTable(tableName);

        entry.WithOwner().HasForeignKey(ownerForeignKey);

        entry.HasKey(Id);

        entry.Property<TEntryId>(Id)
            .HasColumnName(Id)
            .HasConversion(toGuid, fromGuid);

        entry.Property(Status).HasConversion<int>().IsRequired();

        // The two clocks every history in RegOS keeps (ADR-037): the business
        // date it happened on, and the moment RegOS learned of it.
        entry.Property(OccurredOn).IsRequired();
        entry.Property(RecordedOnUtc).IsRequired();

        entry.Property(Note).HasMaxLength(noteMaxLength);

        entry.HasIndex(ownerForeignKey);
    }
}
