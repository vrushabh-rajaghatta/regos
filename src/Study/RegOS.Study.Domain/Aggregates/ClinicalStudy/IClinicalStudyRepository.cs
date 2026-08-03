namespace RegOS.Study.Domain.Aggregates.ClinicalStudy;

public interface IClinicalStudyRepository
{
    Task AddAsync(ClinicalStudy study, CancellationToken cancellationToken);

    /// <summary>Tracked — for mutation.</summary>
    Task<ClinicalStudy?> GetByIdAsync(
        ClinicalStudyId id,
        CancellationToken cancellationToken);

    Task UpdateAsync(ClinicalStudy study, CancellationToken cancellationToken);
}
