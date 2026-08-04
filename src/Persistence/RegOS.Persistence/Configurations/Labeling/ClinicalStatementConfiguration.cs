using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Labeling.Domain.Aggregates.ClinicalStatements;

namespace RegOS.Persistence.Configurations.Labeling;

/// <summary>
/// The mapping every clinical statement's population collection shares.
/// </summary>
/// <remarks>
/// <b>Earned, not assumed.</b> EPIC-018 S004's hypothesis was that the second
/// and third uses of <see cref="Population"/> would differ from the first
/// <em>only</em> by table name. They do — the fields, the nullability, the four
/// coded values and the required owner key are identical in all three — so the
/// helper takes one parameter and the falsifier ("if either introduces a
/// different rule or shape, do not abstract it") was not triggered.
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
