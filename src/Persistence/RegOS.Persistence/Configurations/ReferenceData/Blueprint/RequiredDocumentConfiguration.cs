using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.ReferenceData.Domain.DocumentType;

namespace RegOS.Persistence.Configurations.ReferenceData.Blueprint;

public sealed class RequiredDocumentConfiguration
    : IEntityTypeConfiguration<RequiredDocument>
{
    public void Configure(EntityTypeBuilder<RequiredDocument> builder)
    {
        builder.ToTable("RequiredDocuments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new RequiredDocumentId(value));

        // The section this document belongs to. A plain converted column, like
        // the section's own parent pointer — the aggregate guarantees the
        // section lives in the same version.
        builder.Property(x => x.SectionId)
            .HasConversion(
                id => id.Value,
                value => new TemplateSectionId(value))
            .IsRequired();

        builder.Property(x => x.DocumentTypeId)
            .HasConversion(
                id => id.Value,
                value => new DocumentTypeId(value))
            .IsRequired();

        builder.Property(x => x.IsMandatory)
            .IsRequired();

        builder.Property(x => x.Order)
            .IsRequired();

        // A real FK to the controlled vocabulary: a document type still required
        // by a blueprint cannot be deleted. No navigation — the reference is by id.
        builder.HasOne<DocumentType>()
            .WithMany()
            .HasForeignKey(x => x.DocumentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Shadow FK to the owning version; the relationship binds to it in
        // RegulatoryTemplateVersionConfiguration.
        builder.Property<RegulatoryTemplateVersionId>("RegulatoryTemplateVersionId")
            .HasConversion(
                id => id.Value,
                value => new RegulatoryTemplateVersionId(value));

        builder.HasIndex("RegulatoryTemplateVersionId");

        // One requirement per (section, document type) within a version.
        builder.HasIndex(
                nameof(RequiredDocument.SectionId),
                nameof(RequiredDocument.DocumentTypeId))
            .IsUnique();
    }
}
