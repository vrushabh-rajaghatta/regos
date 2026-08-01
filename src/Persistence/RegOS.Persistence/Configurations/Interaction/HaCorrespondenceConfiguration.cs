using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Interaction.Domain.Correspondence;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.Regulatory.Correspondence;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Primitives;
using RegOS.Submission.Domain.Submission;

using AuthorityAggregate = RegOS.ReferenceData.Domain.Regulatory.Authority.Authority;
using CorrespondenceTypeEntity = RegOS.ReferenceData.Domain.Regulatory.Correspondence.CorrespondenceType;

namespace RegOS.Persistence.Configurations.Interaction;

public sealed class HaCorrespondenceConfiguration
    : IEntityTypeConfiguration<HaCorrespondence>
{
    public void Configure(EntityTypeBuilder<HaCorrespondence> builder)
    {
        builder.ToTable("HaCorrespondence");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => HaCorrespondenceId.From(value));

        builder.Property(x => x.TenantId)
            .HasConversion(
                id => id.Value,
                value => TenantId.From(value))
            .IsRequired();

        builder.HasIndex(x => x.TenantId);

        builder.Property(x => x.AuthorityId)
            .HasConversion(
                id => id.Value,
                value => new AuthorityId(value))
            .IsRequired();

        builder.Property(x => x.CorrespondenceTypeId)
            .HasConversion(
                id => id.Value,
                value => new CorrespondenceTypeId(value))
            .IsRequired();

        builder.Property(x => x.AuthorityDivisionId)
            .HasConversion(
                id => id!.Value.Value,
                value => new AuthorityDivisionId(value));

        builder.Property(x => x.Direction)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Subject)
            .HasMaxLength(HaCorrespondence.SubjectMaxLength)
            .IsRequired();

        builder.Property(x => x.OccurredOn)
            .IsRequired();

        builder.Property(x => x.ResponseDueOn);

        builder.Property(x => x.AuthorityReference)
            .HasMaxLength(HaCorrespondence.ReferenceMaxLength);

        // The three anchors are nullable by design — an interaction that
        // cannot be filed against anything is still a real interaction.
        builder.Property(x => x.RegulatoryApplicationId)
            .HasConversion(
                id => id!.Value.Value,
                value => new RegulatoryApplicationId(value));

        builder.Property(x => x.SubmissionId)
            .HasConversion(
                id => id!.Value.Value,
                value => new SubmissionId(value));

        builder.Property(x => x.RegistrationId)
            .HasConversion(
                id => id!.Value.Value,
                value => new RegistrationId(value));

        builder.Property(x => x.RecordedOnUtc)
            .IsRequired();

        // The two hot paths: the correspondence list is always newest-first,
        // and the due view scans response dates.
        builder.HasIndex(x => new { x.TenantId, x.OccurredOn });
        builder.HasIndex(x => new { x.TenantId, x.ResponseDueOn });
        builder.HasIndex(x => x.RegulatoryApplicationId);

        builder.HasOne<AuthorityAggregate>()
            .WithMany()
            .HasForeignKey(x => x.AuthorityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CorrespondenceTypeEntity>()
            .WithMany()
            .HasForeignKey(x => x.CorrespondenceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Owned by the letter: loaded with it, saved with it, deleted with it.
        builder.OwnsMany(x => x.Attachments, attachment =>
        {
            attachment.ToTable("CorrespondenceAttachments");

            attachment.WithOwner().HasForeignKey("HaCorrespondenceId");

            attachment.HasKey(x => x.Id);

            attachment.Property(x => x.Id)
                .HasColumnName("Id")
                .HasConversion(
                    id => id.Value,
                    value => CorrespondenceAttachmentId.From(value));

            attachment.Property(x => x.OriginalFileName)
                .HasMaxLength(CorrespondenceAttachment.FileNameMaxLength)
                .IsRequired();

            attachment.Property(x => x.ContentType)
                .HasMaxLength(255)
                .IsRequired();

            attachment.Property(x => x.FileSizeBytes)
                .IsRequired();

            attachment.Property(x => x.StoragePath)
                .HasMaxLength(500)
                .IsRequired();

            attachment.Property(x => x.UploadedOnUtc)
                .IsRequired();

            attachment.HasIndex("HaCorrespondenceId");
        });

        builder.Navigation(x => x.Attachments).AutoInclude();

        // The third near-identical owned-collection block in the codebase, and
        // the second in this one file. This — not the entry type — is where the
        // duplication that actually costs maintenance lives; see the S003
        // extraction review.
        builder.OwnsMany(x => x.Questions, question =>
        {
            question.ToTable("HaQuestions");

            question.WithOwner().HasForeignKey("HaCorrespondenceId");

            question.HasKey(x => x.Id);

            question.Property(x => x.Id)
                .HasColumnName("Id")
                .HasConversion(
                    id => id.Value,
                    value => HaQuestionId.From(value));

            question.Property(x => x.Number)
                .HasMaxLength(HaQuestion.NumberMaxLength)
                .IsRequired();

            question.Property(x => x.Text)
                .HasMaxLength(HaQuestion.TextMaxLength)
                .IsRequired();

            question.Property(x => x.TargetResponseOn);

            question.Property(x => x.ResponseText)
                .HasMaxLength(HaQuestion.ResponseMaxLength);

            question.Property(x => x.CurrentStatus)
                .HasConversion<int>()
                .IsRequired();

            question.Ignore(x => x.RespondedOn);

            question.HasIndex("HaCorrespondenceId");

            question.OwnsMany(x => x.History, entry =>
            {
                entry.ToTable("HaQuestionStatusEntries");

                entry.WithOwner().HasForeignKey("HaQuestionId");

                entry.HasKey(x => x.Id);

                entry.Property(x => x.Id)
                    .HasColumnName("Id")
                    .HasConversion(
                        id => id.Value,
                        value => new HaQuestionStatusEntryId(value));

                entry.Property(x => x.Status)
                    .HasConversion<int>()
                    .IsRequired();

                entry.Property(x => x.OccurredOn).IsRequired();
                entry.Property(x => x.RecordedOnUtc).IsRequired();

                entry.Property(x => x.Note)
                    .HasMaxLength(HaQuestionStatusEntry.NoteMaxLength);

                entry.HasIndex("HaQuestionId");
            });

            question.Navigation(x => x.History).AutoInclude();
        });

        builder.Navigation(x => x.Questions).AutoInclude();

        builder.HasOne<AuthorityDivision>()
            .WithMany()
            .HasForeignKey(x => x.AuthorityDivisionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
