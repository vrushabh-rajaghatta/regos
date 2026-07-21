using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Primitives;
using RegOS.Submission.Domain.Submission;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;
using RegulatoryApplicationAggregate = RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;
using SubmissionTypeAggregate = RegOS.ReferenceData.Domain.SubmissionType.SubmissionType;

namespace RegOS.Persistence.Configurations.Submission;

public sealed class SubmissionConfiguration
    : IEntityTypeConfiguration<SubmissionAggregate>
{
    public void Configure(EntityTypeBuilder<SubmissionAggregate> builder)
    {
        builder.ToTable("Submissions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new SubmissionId(value));

        builder.Property(x => x.ApplicationId)
            .HasConversion(
                id => id.Value,
                value => new RegulatoryApplicationId(value))
            .IsRequired();

        builder.Property(x => x.SubmissionTypeId)
            .HasConversion(
                id => id.Value,
                value => new SubmissionTypeId(value))
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedOn)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Null until published.
        builder.Property(x => x.PublishedAt)
            .HasColumnType("timestamp with time zone");

        // Cross-aggregate foreign keys. The domain exposes no navigation
        // properties, but EF still models the relationships.
        //
        // Application: an Application with Submissions must never be
        // deleted out from under them.
        builder.HasOne<RegulatoryApplicationAggregate>()
            .WithMany()
            .HasForeignKey(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        // SubmissionType is reference data. Historical Submissions stay
        // valid even when a type is deactivated — inactive, not deleted.
        builder.HasOne<SubmissionTypeAggregate>()
            .WithMany()
            .HasForeignKey(x => x.SubmissionTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Secondary indexes on the cross-aggregate references.
        // The owning tenant (ADR-031), copied from the parent application at
        // creation. Held by value, no FK to Tenants.
        builder.Property(x => x.TenantId)
            .HasConversion(
                id => id.Value,
                value => new TenantId(value))
            .IsRequired();

        builder.HasIndex(x => x.TenantId);

        builder.HasIndex(x => x.ApplicationId);
        builder.HasIndex(x => x.SubmissionTypeId);

        // Ownership: Submission (1) -> SubmissionDocuments (N). The child has
        // no FK property, so EF uses a shadow "SubmissionId". Deleting a
        // submission deletes its attachments.
        builder.HasMany(x => x.Documents)
            .WithOne()
            .HasForeignKey("SubmissionId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(SubmissionAggregate.Documents))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
