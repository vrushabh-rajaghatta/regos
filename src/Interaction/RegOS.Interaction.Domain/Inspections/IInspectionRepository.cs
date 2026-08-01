namespace RegOS.Interaction.Domain.Inspections;

public interface IInspectionRepository
{
    Task AddAsync(Inspection inspection, CancellationToken cancellationToken);

    Task<Inspection?> GetByIdAsync(InspectionId id, CancellationToken cancellationToken);

    Task UpdateAsync(Inspection inspection, CancellationToken cancellationToken);
}
