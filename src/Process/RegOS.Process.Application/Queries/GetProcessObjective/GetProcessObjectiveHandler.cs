using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Process.Domain.Aggregates.ProcessObjectives;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Process.Application.Queries.GetProcessObjective;

/// <summary>
/// One objective, with the history that shows how it got where it is.
/// </summary>
/// <remarks>
/// <b>The market record is not joined at all.</b> A left join was written first,
/// to project its name — and D8's invariant makes that name redundant, because it
/// would be a second copy of the product and country the objective already
/// carries. The nullable id answers <em>"does one exist yet?"</em> on its own.
/// </remarks>
public sealed class GetProcessObjectiveHandler
{
    private readonly RegOSDbContext _dbContext;

    public GetProcessObjectiveHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProcessObjectiveDetails> HandleAsync(
        GetProcessObjectiveQuery query,
        CancellationToken cancellationToken = default)
    {
        var id = ProcessObjectiveId.From(query.Id);

        var row = await (
            from objective in _dbContext.ProcessObjectives.AsNoTracking()
            where objective.Id == id
            join product in _dbContext.Products
                on objective.GlobalProductId equals product.Id
            join country in _dbContext.Countries
                on objective.CountryId equals country.Id
            select new
            {
                objective.Id,
                objective.Name,
                objective.Rationale,
                objective.GlobalProductId,
                ProductName = product.Name.Value,
                CountryCode = country.Code,
                CountryName = country.Name,
                objective.MedicinalProductId,
                objective.RegulatoryApplicationId,
                objective.OwnerUserId,
                objective.CurrentStatus,
                objective.TargetCompletionOn,
                History = objective.History
                    .Select(x => new
                    {
                        x.Status,
                        x.OccurredOn,
                        x.RecordedOnUtc,
                        x.Note,
                        x.Id
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("That objective does not exist.");

        // Oldest first — a history is read forwards. RecordedOnUtc breaks a tie
        // between two things recorded on the same business date, and the entry
        // id makes the ordering total whatever the clock did.
        var history = row.History
            .OrderBy(x => x.OccurredOn)
            .ThenBy(x => x.RecordedOnUtc)
            .ThenBy(x => x.Id.Value)
            .ToList();

        return new ProcessObjectiveDetails(
            row.Id.Value,
            row.Name,
            row.Rationale,
            row.GlobalProductId.Value,
            row.ProductName,
            row.CountryCode,
            row.CountryName,
            row.MedicinalProductId?.Value,
            row.RegulatoryApplicationId?.Value,
            row.OwnerUserId?.Value,
            row.CurrentStatus.ToString(),
            history[0].OccurredOn,
            row.TargetCompletionOn,
            history
                .Where(x => x.Status == ProcessObjectiveStatus.Achieved)
                .Select(x => (DateOnly?)x.OccurredOn)
                .FirstOrDefault(),
            [.. history.Select(x => new ProcessObjectiveHistoryEntry(
                x.Status.ToString(),
                x.OccurredOn,
                x.RecordedOnUtc,
                x.Note))]);
    }
}
