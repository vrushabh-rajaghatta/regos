using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;

namespace RegOS.Persistence.Configurations.Labeling;

/// <summary>
/// The mapping every clinical statement's population collection shares.
/// </summary>
/// <remarks>
/// <b>Extracted after demonstrating schema equivalence across three owners</b>
/// — not to reduce duplication, and the distinction matters because it is what
/// makes the justification durable.
/// <para>
/// The evidence is EF's own: replacing S003's hand-written
/// <c>PopulationConfiguration</c> with this helper generated <b>no migration
/// for <c>IndicationPopulations</c> at all</b>. A migration is the ORM's
/// description of the persisted model, so a mapping that produces an empty diff
/// is not "equivalent by inspection" — it is equivalent according to the thing
/// that defines the schema. Had a nullability, a length or an index differed,
/// the migration would have said so.
/// </para>
/// <para>
/// EPIC-018 S004's falsifier — <em>"if either aggregate introduces a different
/// rule or a different shape, do not abstract it"</em> — was therefore not
/// triggered. If a fourth owner ever needs a different column, the honest move
/// is a second helper, not a parameter on this one.
/// </para>
/// <para>
/// <b>Persistence mechanics only.</b> There is no shared domain base type across
/// the three roots, and there is no shared table: an owned entity is tracked
/// against exactly one owner (ADR-058), so each statement keeps its own rows and
/// its own foreign key. Sharing the table would have needed a discriminator and
/// would have lost the constraint that makes the ownership real.
/// </para>
/// <para>
/// <b>Owned, not a standalone entity.</b> Three aggregates cannot own one
/// entity type with three tables — EF scopes an owned type per owner, which is
/// what makes one CLR class into three entity types. That is why S003's
/// <c>PopulationConfiguration</c> is gone rather than copied twice.
/// </para>
/// </remarks>
internal static class ClinicalStatementConfiguration
{
    /// <param name="table">
    /// The only thing that differs between the three. The owner key column is
    /// derived from it so the two cannot drift apart.
    /// </param>
    internal static void Populations<TOwner>(
        OwnedNavigationBuilder<TOwner, Population> populations,
        string table,
        string ownerKey)
        where TOwner : class
    {
        populations.ToTable(table);

        populations.WithOwner().HasForeignKey(ownerKey);

        populations.HasKey(x => x.Id);

        populations.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new PopulationId(value))
            .ValueGeneratedNever();

        populations.Property(x => x.AgeLow);
        populations.Property(x => x.AgeHigh);

        populations.Property(x => x.Description)
            .HasMaxLength(Population.DescriptionMaxLength);

        populations.OwnsOne(
            x => x.AgeUnit,
            CodedConceptColumns.Of<Population>("AgeUnit", required: false));

        populations.OwnsOne(
            x => x.Gender,
            CodedConceptColumns.Of<Population>("Gender"));

        populations.Navigation(x => x.Gender).IsRequired();

        populations.OwnsOne(
            x => x.PhysiologicalCondition,
            CodedConceptColumns.Of<Population>(
                "PhysiologicalCondition", required: false));

        populations.HasIndex(ownerKey);
    }
}
