using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.SharedKernel.Exceptions;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Queries.GetApplicationContacts;

/// <summary>
/// The current contacts for an application, read from the latest published
/// sequence.
/// </summary>
/// <remarks>
/// <b>This query is why there is no application-level contact model</b>
/// (ADR-048). Under the cumulative model (ADR-045) the latest published
/// sequence <em>is</em> the current regulatory state, so a stored copy of
/// "the application's contacts" could only ever differ from this by being
/// stale. The same argument that removed <c>SubmissionSnapshot</c> in S002.
/// </remarks>
public sealed class GetApplicationContactsHandler
{
    private readonly RegOSDbContext _dbContext;

    public GetApplicationContactsHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApplicationContacts> HandleAsync(
        GetApplicationContactsQuery query,
        CancellationToken cancellationToken)
    {
        var applicationExists = await _dbContext.RegulatoryApplications
            .AsNoTracking()
            .AnyAsync(x => x.Id == query.ApplicationId, cancellationToken);

        if (!applicationExists)
            throw new NotFoundException(
                SubmissionRuleErrors.ApplicationDoesNotExist);

        // The latest filing, by sequence number rather than by date: number
        // order is transmission order by construction (ADR-044), and a
        // backdated import must not become "current" by having a later
        // timestamp.
        var latest = await _dbContext.Submissions
            .AsNoTracking()
            .Where(x => x.ApplicationId == query.ApplicationId
                && x.SequenceNumber != null)
            .OrderByDescending(x => x.SequenceNumber)
            .Select(x => new { x.Id, x.SequenceNumber })
            .FirstOrDefaultAsync(cancellationToken);

        // Nothing published: there is no filing, so there is nobody named on
        // one. An empty answer, not a missing one.
        if (latest is null)
            return new ApplicationContacts(null, []);

        var rows = await (
            from role in _dbContext.Set<SubmissionRole>().AsNoTracking()
            where EF.Property<SubmissionId>(role, "SubmissionId") == latest.Id
            join contact in _dbContext.Contacts
                on role.ContactId equals contact.Id
            join organization in _dbContext.Organizations
                on contact.OrganizationId equals organization.Id
            join contactRole in _dbContext.ContactRoles
                on role.RoleId equals contactRole.Id
            orderby contactRole.Name, contact.LastName, contact.FirstName
            select new
            {
                ContactId = contact.Id,
                contact.FirstName,
                contact.LastName,
                contact.Title,
                OrganizationName = organization.LegalName,
                RoleId = contactRole.Id,
                RoleName = contactRole.Name,
            }).ToListAsync(cancellationToken);

        return new ApplicationContacts(
            latest.SequenceNumber,
            rows
                .Select(row => new ApplicationContact(
                    row.ContactId.Value,
                    $"{row.FirstName} {row.LastName}",
                    row.Title,
                    row.OrganizationName,
                    row.RoleId.Value,
                    row.RoleName))
                .ToList());
    }
}
