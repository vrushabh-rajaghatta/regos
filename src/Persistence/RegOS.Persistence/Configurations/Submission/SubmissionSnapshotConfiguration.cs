using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Submission.Domain.Snapshot;
using RegOS.Submission.Domain.Submission;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;

namespace RegOS.Persistence.Configurations.Submission;

public sealed class SubmissionSnapshotConfiguration
    : IEntityTypeConfiguration<SubmissionSnapshot>
{
    public void Configure(EntityTypeBuilder<SubmissionSnapshot> builder)
    {
        builder.ToTable("SubmissionSnapshots");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new SubmissionSnapshotId(value));

        builder.Property(x => x.SubmissionId)
            .HasConversion(
                id => id.Value,
                value => new SubmissionId(value))
            .IsRequired();

        // The snapshot references its submission. A submission that has been
        // published (and so has a snapshot) must never be deleted out from under
        // its historical record.
        builder.HasOne<SubmissionAggregate>()
            .WithMany()
            .HasForeignKey(x => x.SubmissionId)
            .OnDelete(DeleteBehavior.Restrict);

        // One submission, one publish, one snapshot — pushed into the database.
        builder.HasIndex(x => x.SubmissionId)
            .IsUnique();

        // Ownership: SubmissionSnapshot (1) -> SnapshotDocuments (N). The child
        // holds no FK property, so EF uses a shadow "SubmissionSnapshotId". Cascade
        // mirrors the Submission -> SubmissionDocuments convention; the guarantee
        // that history is never deleted lives in the domain/application, not here.
        builder.HasMany(x => x.Documents)
            .WithOne()
            .HasForeignKey("SubmissionSnapshotId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(SubmissionSnapshot.Documents))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
