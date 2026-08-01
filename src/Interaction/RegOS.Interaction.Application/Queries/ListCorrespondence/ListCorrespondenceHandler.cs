using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.Interaction.Application.Queries.ListCorrespondence;

/// <summary>
/// Reads the DbContext directly with <c>AsNoTracking()</c> — a query handler
/// never loads an aggregate (ADR-016). Tenant scoping is the fail-closed global
/// filter's job, not this handler's (ADR-031).
/// </summary>
public sealed class ListCorrespondenceHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListCorrespondenceHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CorrespondenceSummary>> HandleAsync(
        ListCorrespondenceQuery query,
        CancellationToken cancellationToken)
    {
        var correspondence = _dbContext.HaCorrespondence.AsNoTracking();

        if (query.AuthorityId is { } authorityId)
            correspondence = correspondence.Where(x => x.AuthorityId == authorityId);

        if (query.CorrespondenceTypeId is { } typeId)
            correspondence = correspondence.Where(x => x.CorrespondenceTypeId == typeId);

        if (query.Direction is { } direction)
            correspondence = correspondence.Where(x => x.Direction == direction);

        if (query.RegulatoryApplicationId is { } applicationId)
            correspondence = correspondence
                .Where(x => x.RegulatoryApplicationId == applicationId);

        // The names come from reference data and from the application; the
        // correspondence stores ids only (ES-014). Composing them here is a
        // read model, which crosses contexts freely — projection is not write
        // ownership (ADR-039 principle 7).
        return await correspondence
            .OrderByDescending(x => x.OccurredOn)
            .ThenByDescending(x => x.RecordedOnUtc)
            .Join(
                _dbContext.Authorities.AsNoTracking(),
                x => x.AuthorityId,
                a => a.Id,
                (x, a) => new { Correspondence = x, Authority = a })
            .Join(
                _dbContext.CorrespondenceTypes.AsNoTracking(),
                x => x.Correspondence.CorrespondenceTypeId,
                t => t.Id,
                (x, t) => new { x.Correspondence, x.Authority, Type = t })
            .GroupJoin(
                _dbContext.RegulatoryApplications.AsNoTracking(),
                x => x.Correspondence.RegulatoryApplicationId,
                a => a.Id,
                (x, applications) => new { x.Correspondence, x.Authority, x.Type, Applications = applications })
            .SelectMany(
                x => x.Applications.DefaultIfEmpty(),
                (x, application) => new CorrespondenceSummary(
                    x.Correspondence.Id.Value,
                    x.Correspondence.Direction.ToString(),
                    x.Correspondence.Subject,
                    x.Correspondence.OccurredOn,
                    x.Correspondence.ResponseDueOn,
                    x.Correspondence.AuthorityReference,
                    x.Authority.Id.Value,
                    x.Authority.Name,
                    x.Type.Id.Value,
                    x.Type.Name,
                    application != null ? application.Id.Value : null,
                    application != null ? application.ApplicationNumber : null))
            .ToListAsync(cancellationToken);
    }
}
