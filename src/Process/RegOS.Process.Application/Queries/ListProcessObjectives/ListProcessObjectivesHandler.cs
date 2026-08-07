using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Process.Domain.Aggregates.ProcessObjectives;

namespace RegOS.Process.Application.Queries.ListProcessObjectives;

/// <summary>
/// <em>"What are we trying to achieve, and where?"</em> — the epic's second
/// question, answered across a whole tenant.
/// </summary>
/// <remarks>
/// <c>CurrentStatus</c> is stored rather than derived, and this is the query that
/// earns it: filtering "not achieved and not abandoned" across every objective a
/// tenant holds would otherwise walk every history.
/// </remarks>
public sealed class ListProcessObjectivesHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListProcessObjectivesHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProcessObjectiveSummary>> HandleAsync(
        ListProcessObjectivesQuery query,
        CancellationToken cancellationToken = default)
    {
        var objectives = _dbContext.ProcessObjectives.AsNoTracking();

        if (!query.IncludeClosed)
        {
            objectives = objectives.Where(
                x => x.CurrentStatus != ProcessObjectiveStatus.Achieved
                    && x.CurrentStatus != ProcessObjectiveStatus.Abandoned);
        }

        var rows = await (
            from objective in objectives
            join product in _dbContext.Products
                on objective.GlobalProductId equals product.Id
            join country in _dbContext.Countries
                on objective.CountryId equals country.Id
            orderby country.Code, objective.Name, objective.Id
            select new
            {
                objective.Id,
                objective.Name,
                ProductName = product.Name.Value,
                CountryCode = country.Code,
                CountryName = country.Name,
                objective.CurrentStatus,
                objective.TargetCompletionOn,
                objective.MedicinalProductId,
                objective.OwnerUserId,
                History = objective.History
                    .Select(x => new { x.Status, x.OccurredOn })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return
        [
            .. rows.Select(row => new ProcessObjectiveSummary(
                row.Id.Value,
                row.Name,
                row.ProductName,
                row.CountryCode,
                row.CountryName,
                row.CurrentStatus.ToString(),
                // Derived from the history rather than stored beside it, the
                // same call Commitment.GivenOn made: a stored copy could
                // disagree with the entries it summarises.
                // Deterministic: the first entry is the minimum business date,
                // and Create writes exactly one before the aggregate exists.
                row.History.Min(x => x.OccurredOn),
                row.TargetCompletionOn,
                row.History
                    .Where(x => x.Status == ProcessObjectiveStatus.Achieved)
                    .Select(x => (DateOnly?)x.OccurredOn)
                    .FirstOrDefault(),
                row.MedicinalProductId is not null,
                row.OwnerUserId?.Value))
        ];
    }
}
