namespace RegOS.Interaction.Domain.Commitments;

public interface ICommitmentRepository
{
    Task AddAsync(Commitment commitment, CancellationToken cancellationToken);

    /// <summary>Tracked, with history — for mutation. Reads use the DbContext (ADR-016).</summary>
    Task<Commitment?> GetByIdAsync(CommitmentId id, CancellationToken cancellationToken);

    Task UpdateAsync(Commitment commitment, CancellationToken cancellationToken);
}
