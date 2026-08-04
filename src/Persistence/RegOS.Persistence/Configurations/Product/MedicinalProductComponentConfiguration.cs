using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Persistence.Configurations.Product;

public sealed class MedicinalProductComponentConfiguration
    : IEntityTypeConfiguration<MedicinalProductComponent>
{
    public void Configure(EntityTypeBuilder<MedicinalProductComponent> builder)
    {
        builder.ToTable("MedicinalProductComponents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value, value => new MedicinalProductComponentId(value))
            .ValueGeneratedNever();

        // The owning tenant (ADR-031), held by value; no FK to Tenants.
        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(x => x.MedicinalProductId)
            .HasConversion(id => id.Value, value => new MedicinalProductId(value))
            .IsRequired();

        // Nullable on purpose here, unlike the composition's parent key: null
        // *means* something — this is what the patient is handed rather than
        // something inside it — so the column is expressing a domain fact, not
        // an accident of shadow-key defaults.
        builder.Property(x => x.ParentComponentId)
            .HasConversion(
                id => id!.Value, value => new MedicinalProductComponentId(value));

        builder.Property(x => x.Name)
            .HasMaxLength(MedicinalProductComponent.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(MedicinalProductComponent.DescriptionMaxLength);

        builder.Property(x => x.Quantity)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.OwnsOne(x => x.ComponentType, c => Columns(c, "ComponentType"));
        builder.Navigation(x => x.ComponentType).IsRequired();

        builder.OwnsOne(
            x => x.UnitOfPresentation, c => Columns(c, "UnitOfPresentation"));

        builder.OwnsOne(x => x.DoseForm, c => Columns(c, "DoseForm"));

        builder.Property(x => x.CreatedOn)
            .IsRequired();

        // Self-referencing, and deliberately Restrict rather than Cascade: the
        // domain refuses to remove a component that still holds others, so a
        // cascade here would be a second, silent answer to a question the
        // aggregate already answers out loud.
        //
        // Nothing in the schema says the graph is acyclic — Postgres cannot
        // express that for an adjacency list without a trigger, and the rule
        // lives in ComponentTree instead.
        builder.HasOne<MedicinalProductComponent>()
            .WithMany()
            .HasForeignKey(x => x.ParentComponentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<MedicinalProduct>()
            .WithMany()
            .HasForeignKey(x => x.MedicinalProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Every rule about shape loads by market, so this is the index that
        // every write as well as every read goes through.
        builder.HasIndex(x => x.MedicinalProductId);

        builder.HasIndex(x => x.TenantId);
    }

    /// <remarks>
    /// The column names are the only thing that differs between the three, so
    /// they are the only thing parameterised. Anything more — a helper that
    /// also chose the navigation — would need the three call sites back to say
    /// which navigation it meant.
    /// </remarks>
    private static void Columns(
        OwnedNavigationBuilder<MedicinalProductComponent, CodedConcept> owned,
        string prefix)
    {
        owned.Property(x => x.System)
            .HasColumnName($"{prefix}System")
            .HasMaxLength(CodedConcept.SystemMaxLength);

        owned.Property(x => x.Code)
            .HasColumnName($"{prefix}Code")
            .HasMaxLength(CodedConcept.CodeMaxLength);

        owned.Property(x => x.Display)
            .HasColumnName($"{prefix}Display")
            .HasMaxLength(CodedConcept.DisplayMaxLength);
    }
}
