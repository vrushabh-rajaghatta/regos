namespace RegOS.Study.Domain.Aggregates.NonClinicalStudy;

public interface INonClinicalStudyRepository
{
    Task AddAsync(NonClinicalStudy study, CancellationToken cancellationToken);

    /// <summary>Tracked — for mutation.</summary>
    Task<NonClinicalStudy?> GetByIdAsync(
        NonClinicalStudyId id,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        NonClinicalStudy study,
        CancellationToken cancellationToken);
}
