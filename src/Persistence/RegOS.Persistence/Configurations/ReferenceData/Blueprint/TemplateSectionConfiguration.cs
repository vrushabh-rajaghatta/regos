using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.ReferenceData.Domain.Blueprint;

namespace RegOS.Persistence.Configurations.ReferenceData.Blueprint;

public sealed class TemplateSectionConfiguration
    : IEntityTypeConfiguration<TemplateSection>
{
    public void Configure(EntityTypeBuilder<TemplateSection> builder)
    {
        builder.ToTable("TemplateSections");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new TemplateSectionId(value));

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(300)
            .IsRequired();

        // Adjacency list: the parent pointer is a plain converted column, not a
        // self-referential FK. The aggregate guarantees the parent belongs to
        // the same version, and a self-FK would add an insert-order cycle.
        builder.Property(x => x.ParentSectionId)
            .HasConversion(
                id => id != null ? id.Value.Value : (Guid?)null,
                value => value != null
                    ? new TemplateSectionId(value.Value)
                    : (TemplateSectionId?)null);

        builder.Property(x => x.Order)
            .IsRequired();

        // Where a document placed in this section is written on disk, relative
        // to its parent's folder. Nullable, and the null is load-bearing: it
        // means the specification that says so has not been read (ICH Appendix 4
        // is not in this repository), never "derive it from the code".
        //
        // One segment is capped at ICH Appendix 2's 64 characters, but the value
        // may chain several — FDA's Module 1 root is "m1/us", one section and
        // two directories — so the column is wider than a single segment.
        builder.Property(x => x.EctdFolder)
            .HasMaxLength(256);

        // Who chose the name beside it — a specification, or RegOS (ADR-052).
        // Stored so that "ICH published this" and "we made this up because
        // nothing prescribed one" never read alike. Null exactly when the
        // folder is null; the aggregate enforces the pairing.
        builder.Property(x => x.EctdFolderSource)
            .HasConversion<int?>();

        // What this section is called in each backbone. Two columns because a
        // backbone is a contract (E16): the names come from different DTDs, and
        // ICH declares one Module 1 element where FDA declares 147.
        builder.Property(x => x.IchElement).HasMaxLength(256);
        builder.Property(x => x.RegionalElement).HasMaxLength(256);

        builder.Ignore(x => x.HasEctdPlacement);

        // Shadow FK to the owning version; the relationship binds to it in
        // RegulatoryTemplateVersionConfiguration.
        builder.Property<RegulatoryTemplateVersionId>("RegulatoryTemplateVersionId")
            .HasConversion(
                id => id.Value,
                value => new RegulatoryTemplateVersionId(value));

        builder.HasIndex("RegulatoryTemplateVersionId");

        // Section codes are unique within a version.
        builder.HasIndex(
                "RegulatoryTemplateVersionId",
                nameof(TemplateSection.Code))
            .IsUnique();
    }
}
