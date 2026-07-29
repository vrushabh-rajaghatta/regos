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
