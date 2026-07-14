using RegOS.ReferenceData.Domain.SubmissionType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

namespace RegOS.Submission.Domain.Submission;

public sealed class Submission
{
    private Submission(
        SubmissionId id,
        RegulatoryApplicationId applicationId,
        SubmissionTypeId submissionTypeId,
        string name,
        DateTime createdOn)
    {
        Id = id;
        ApplicationId = applicationId;
        SubmissionTypeId = submissionTypeId;
        Name = name;
        Status = SubmissionStatus.Draft;
        CreatedOn = createdOn;
    }

    public SubmissionId Id { get; }

    public RegulatoryApplicationId ApplicationId { get; }

    public SubmissionTypeId SubmissionTypeId { get; }

    public string Name { get; private set; }

    public SubmissionStatus Status { get; private set; }

    public DateTime CreatedOn { get; }

    public static Submission Create(
        RegulatoryApplicationId applicationId,
        SubmissionTypeId submissionTypeId,
        string name)
    {
        if (applicationId == default)
            throw new ArgumentException(
                SubmissionErrors.ApplicationRequired,
                nameof(applicationId));

        if (submissionTypeId == default)
            throw new ArgumentException(
                SubmissionErrors.SubmissionTypeRequired,
                nameof(submissionTypeId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                SubmissionErrors.NameRequired,
                nameof(name));

        return new Submission(
            SubmissionId.New(),
            applicationId,
            submissionTypeId,
            name.Trim(),
            DateTime.UtcNow);
    }
}
