using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.ReferenceData.Domain.Blueprint;

namespace RegOS.Persistence.Configurations.ReferenceData.Blueprint;

public sealed class ValidationRuleConfiguration
    : IEntityTypeConfiguration<ValidationRule>
{
    public void Configure(EntityTypeBuilder<ValidationRule> builder)
    {
        builder.ToTable("ValidationRules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new ValidationRuleId(value));

        builder.Property(x => x.Code)
            .HasMaxLength(ValidationRule.CodeMaxLength)
            .IsRequired();

        builder.Property(x => x.RuleType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Severity)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Message)
            .HasMaxLength(ValidationRule.MessageMaxLength)
            .IsRequired();

        // Optional section target. A plain converted column, like the section's
        // own parent pointer — the aggregate guarantees it lives in this version.
        builder.Property(x => x.SectionId)
            .HasConversion(
                id => id != null ? id.Value.Value : (Guid?)null,
                value => value != null
                    ? new TemplateSectionId(value.Value)
                    : (TemplateSectionId?)null);

        builder.Property(x => x.Parameters)
            .HasMaxLength(ValidationRule.ParametersMaxLength);

        builder.Property(x => x.Order)
            .IsRequired();

        // Shadow FK to the owning version; the relationship binds to it in
        // RegulatoryTemplateVersionConfiguration.
        builder.Property<RegulatoryTemplateVersionId>("RegulatoryTemplateVersionId")
            .HasConversion(
                id => id.Value,
                value => new RegulatoryTemplateVersionId(value));

        builder.HasIndex("RegulatoryTemplateVersionId");

        // Rule codes are unique within a version.
        builder.HasIndex(
                "RegulatoryTemplateVersionId",
                nameof(ValidationRule.Code))
            .IsUnique();
    }
}
