using Microsoft.EntityFrameworkCore;

using RegOS.Labeling.Application.Queries.ListIndications;
using RegOS.Persistence;

namespace RegOS.Labeling.Application.Queries.ListDrugInteractions;

public sealed class ListDrugInteractionsHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListDrugInteractionsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <remarks>
    /// Starts at the filtered root. The substance name is joined rather than
    /// stored: a copy beside the id could disagree with the catalogue, which is
    /// the whole reason the link is an id (ADR-058 §1).
    /// </remarks>
    public async Task<IReadOnlyList<DrugInteractionSummary>> HandleAsync(
        ListDrugInteractionsQuery query,
        CancellationToken cancellationToken)
    {
        var substances = _dbContext.Substances.AsNoTracking();

        return await _dbContext.Interactions
            .AsNoTracking()
            .Where(x => x.MedicinalProductId == query.MedicinalProductId)
            .OrderBy(x => x.CreatedOnUtc)
            .ThenBy(x => x.Id)
            .Select(interaction => new DrugInteractionSummary(
                interaction.Id.Value,
                interaction.InteractionType.Code,
                interaction.InteractionType.Display,
                interaction.LabelText,
                interaction.Management,
                interaction.Severity == null
                    ? null
                    : interaction.Severity.Code,
                interaction.Severity == null
                    ? null
                    : interaction.Severity.Display,

                interaction.Interactants
                    .Select(interactant => new InteractantSummary(
                        interactant.Id.Value,
                        interactant.Description,
                        interactant.SubstanceId == null
                            ? (Guid?)null
                            : interactant.SubstanceId.Value,
                        substances
                            .Where(s => s.Id == interactant.SubstanceId)
                            .Select(s => s.Name)
                            .FirstOrDefault()))
                    .ToList(),

                interaction.Populations
                    .Select(population => new PopulationSummary(
                        population.Id.Value,
                        population.AgeLow,
                        population.AgeHigh,
                        population.AgeUnit == null
                            ? null
                            : population.AgeUnit.Code,
                        population.AgeUnit == null
                            ? null
                            : population.AgeUnit.Display,
                        population.Gender.Code,
                        population.Gender.Display,
                        population.PhysiologicalCondition == null
                            ? null
                            : population.PhysiologicalCondition.Code,
                        population.PhysiologicalCondition == null
                            ? null
                            : population.PhysiologicalCondition.Display,
                        population.Description))
                    .ToList()))
            .ToListAsync(cancellationToken);
    }
}
