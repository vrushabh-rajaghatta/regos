using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.ReferenceData.Domain.Blueprint;

namespace RegOS.Persistence.Configurations.ReferenceData.Blueprint;

public sealed class RegulatoryTemplateVersionConfiguration
    : IEntityTypeConfiguration<RegulatoryTemplateVersion>
{
    public void Configure(EntityTypeBuilder<RegulatoryTemplateVersion> builder)
    {
        builder.ToTable("RegulatoryTemplateVersions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new RegulatoryTemplateVersionId(value));

        builder.Property(x => x.VersionNumber)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        // Temporal validity — the governance seam. Nullable dates.
        builder.Property(x => x.EffectiveFrom);
        builder.Property(x => x.EffectiveTo);

        builder.Property(x => x.PublishedOnUtc)
            .HasColumnType("timestamp with time zone");

        // Shadow FK to the owning template, declared with the root's
        // strongly-typed id (and its converter) so it is compatible with the
        // template's primary key; the ownership relationship binds to it in
        // RegulatoryTemplateConfiguration.
        builder.Property<RegulatoryTemplateId>("RegulatoryTemplateId")
            .HasConversion(
                id => id.Value,
                value => new RegulatoryTemplateId(value));

        builder.HasIndex("RegulatoryTemplateId");

        // Enforces the aggregate invariant at the database level: version
        // numbers are unique within a template.
        builder.HasIndex(
                "RegulatoryTemplateId",
                nameof(RegulatoryTemplateVersion.VersionNumber))
            .IsUnique();

        // Ownership: version (1) -> sections (N). The child holds a shadow FK.
        builder.HasMany(x => x.Sections)
            .WithOne()
            .HasForeignKey("RegulatoryTemplateVersionId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(RegulatoryTemplateVersion.Sections))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
