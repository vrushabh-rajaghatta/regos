using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Queries.ListSubmissionRoles;

public sealed class ListSubmissionRolesHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListSubmissionRolesHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Who is named on this submission, or null when the submission does not
    /// exist — so the endpoint can 404 rather than return an empty list for a
    /// submission that is not there.
    /// </summary>
    public async Task<IReadOnlyList<SubmissionRoleListItem>?> HandleAsync(
        ListSubmissionRolesQuery query,
        CancellationToken cancellationToken)
    {
        var submissionId = query.SubmissionId;

        var submissionExists = await _dbContext.Submissions
            .AsNoTracking()
            .AnyAsync(s => s.Id == submissionId, cancellationToken);

        if (!submissionExists)
            return null;

        // The naming carries only ids; the person's name, their company and the
        // role's name are read through the aggregates that own them. Reads
        // compose (ADR-039 principle 7) — this is not the Submission context
        // claiming any of those facts.
        var rows = await (
            from role in _dbContext.Set<SubmissionRole>().AsNoTracking()
            where EF.Property<SubmissionId>(role, "SubmissionId") == submissionId
            join contact in _dbContext.Contacts
                on role.ContactId equals contact.Id
            join organization in _dbContext.Organizations
                on contact.OrganizationId equals organization.Id
            join contactRole in _dbContext.ContactRoles
                on role.RoleId equals contactRole.Id
            orderby contactRole.Name, contact.LastName, contact.FirstName, role.Id
            select new
            {
                role.Id,
                contact.FirstName,
                contact.LastName,
                ContactId = contact.Id,
                contact.Title,
                OrganizationName = organization.LegalName,
                RoleId = contactRole.Id,
                RoleName = contactRole.Name,
            }).ToListAsync(cancellationToken);

        return rows
            .Select(row => new SubmissionRoleListItem(
                row.Id.Value,
                row.ContactId.Value,
                $"{row.FirstName} {row.LastName}",
                row.Title,
                row.OrganizationName,
                row.RoleId.Value,
                row.RoleName))
            .ToList();
    }
}
