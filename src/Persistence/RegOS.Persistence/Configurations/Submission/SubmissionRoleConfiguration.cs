using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Organization.Domain.Aggregates.Contact;
using RegOS.ReferenceData.Domain.Organization;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Persistence.Configurations.Submission;

public sealed class SubmissionRoleConfiguration
    : IEntityTypeConfiguration<SubmissionRole>
{
    public void Configure(EntityTypeBuilder<SubmissionRole> builder)
    {
        builder.ToTable("SubmissionRoles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => SubmissionRoleId.From(value));

        builder.Property(x => x.ContactId)
            .HasConversion(id => id.Value, value => ContactId.From(value))
            .IsRequired();

        builder.Property(x => x.RoleId)
            .HasConversion(id => id.Value, value => new ContactRoleId(value))
            .IsRequired();

        // Shadow FK to the owning submission. IsRequired because a
        // reference-type id makes the inferred FK optional, and an optional FK
        // severs instead of deleting (ADR-043 migration note).
        builder.Property<SubmissionId>("SubmissionId")
            .HasConversion(id => id.Value, value => SubmissionId.From(value))
            .IsRequired();

        builder.HasIndex("SubmissionId");

        // Restrict, not cascade: a contact named on a filed sequence is part of
        // the record of that filing. Contacts are retired rather than deleted
        // (ES-018), so this should never fire — and if it does, losing the
        // naming would be worse than the failure.
        builder.HasOne<Contact>()
            .WithMany()
            .HasForeignKey(x => x.ContactId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ContactRole>()
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Mirrors the aggregate: naming the same person as the same thing twice
        // says it twice, not doubly.
        builder.HasIndex(
                "SubmissionId",
                nameof(SubmissionRole.ContactId),
                nameof(SubmissionRole.RoleId))
            .IsUnique();
    }
}
