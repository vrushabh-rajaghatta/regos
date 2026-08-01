using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Interaction.Application.Queries.GetCorrespondence;

public sealed class GetCorrespondenceHandler
{
    private readonly RegOSDbContext _dbContext;

    public GetCorrespondenceHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CorrespondenceDetail> HandleAsync(
        GetCorrespondenceQuery query,
        CancellationToken cancellationToken)
    {
        var detail = await _dbContext.HaCorrespondence
            .AsNoTracking()
            .Where(x => x.Id == query.CorrespondenceId)
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
                _dbContext.AuthorityDivisions.AsNoTracking(),
                x => x.Correspondence.AuthorityDivisionId,
                d => d.Id,
                (x, divisions) => new { x.Correspondence, x.Authority, x.Type, Divisions = divisions })
            .SelectMany(
                x => x.Divisions.DefaultIfEmpty(),
                (x, division) => new { x.Correspondence, x.Authority, x.Type, Division = division })
            .GroupJoin(
                _dbContext.RegulatoryApplications.AsNoTracking(),
                x => x.Correspondence.RegulatoryApplicationId,
                a => a.Id,
                (x, applications) => new { x.Correspondence, x.Authority, x.Type, x.Division, Applications = applications })
            .SelectMany(
                x => x.Applications.DefaultIfEmpty(),
                (x, application) => new CorrespondenceDetail(
                    x.Correspondence.Id.Value,
                    x.Correspondence.Direction.ToString(),
                    x.Correspondence.Subject,
                    x.Correspondence.OccurredOn,
                    x.Correspondence.ResponseDueOn,
                    x.Correspondence.AuthorityReference,
                    x.Correspondence.RecordedOnUtc,
                    x.Authority.Id.Value,
                    x.Authority.Name,
                    x.Type.Id.Value,
                    x.Type.Name,
                    x.Division != null ? x.Division.Id.Value : (Guid?)null,
                    x.Division != null ? x.Division.Name : null,
                    application != null ? application.Id.Value : null,
                    application != null ? application.Name : null,
                    application != null ? application.ApplicationNumber : null,
                    x.Correspondence.SubmissionId != null
                        ? x.Correspondence.SubmissionId!.Value.Value
                        : null,
                    x.Correspondence.RegistrationId != null
                        ? x.Correspondence.RegistrationId!.Value.Value
                        : null))
            .SingleOrDefaultAsync(cancellationToken);

        // Absent, or invisible to this caller — the tenant filter makes those
        // the same answer on purpose (ADR-031).
        return detail
            ?? throw new NotFoundException("Correspondence was not found.");
    }
}
