using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Persistence;
using RegOS.SharedKernel.Exceptions;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Commands.AssignSubmissionRole;

public sealed class AssignSubmissionRoleHandler
{
    private readonly RegOSDbContext _dbContext;
    private readonly ISubmissionRepository _repository;

    public AssignSubmissionRoleHandler(
        RegOSDbContext dbContext,
        ISubmissionRepository repository)
    {
        _dbContext = dbContext;
        _repository = repository;
    }

    public async Task<AssignSubmissionRoleResult> HandleAsync(
        AssignSubmissionRoleCommand command,
        CancellationToken cancellationToken)
    {
        var submission = await _repository.GetByIdAsync(
            command.SubmissionId,
            cancellationToken);

        if (submission is null)
            throw new NotFoundException(
                SubmissionRuleErrors.SubmissionDoesNotExist);

        // The contact is read through the DbContext, whose fail-closed tenant
        // filter (ADR-031) is what stops one tenant naming another's people.
        // No explicit tenant comparison here — a second check would be a second
        // place for the rule to be wrong.
        var contact = await _dbContext.Contacts
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == command.ContactId, cancellationToken);

        if (contact is null)
            throw new NotFoundException(SubmissionRuleErrors.ContactDoesNotExist);

        // "Do not name this person on anything new" is exactly what
        // deactivating a contact means (EPIC-016). Existing namings on filed
        // sequences are untouched — this refuses the new one only.
        if (contact.Status != OrganizationStatus.Active)
            throw new BusinessRuleViolationException(
                SubmissionRuleErrors.ContactNotActive);

        // Platform roles have a null TenantId and a tenant's own roles are
        // filtered; either way, an id that resolves is one this tenant may use.
        var roleExists = await _dbContext.ContactRoles
            .AsNoTracking()
            .AnyAsync(x => x.Id == command.RoleId, cancellationToken);

        if (!roleExists)
            throw new DomainException(SubmissionRuleErrors.ContactRoleDoesNotExist);

        // Whether this submission is still a draft, and whether the person is
        // already named in this role, are both visible from the aggregate.
        var role = submission.AssignRole(command.ContactId, command.RoleId);

        await _repository.UpdateAsync(submission, cancellationToken);

        return new AssignSubmissionRoleResult(role.Id);
    }
}
