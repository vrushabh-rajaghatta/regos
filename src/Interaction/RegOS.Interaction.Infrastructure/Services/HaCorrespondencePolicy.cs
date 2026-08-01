using Microsoft.EntityFrameworkCore;

using RegOS.Interaction.Application.Services;
using RegOS.Persistence;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.Regulatory.Correspondence;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Exceptions;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Interaction.Infrastructure.Services;

public sealed class HaCorrespondencePolicy : IHaCorrespondencePolicy
{
    private readonly RegOSDbContext _dbContext;

    public HaCorrespondencePolicy(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureCanRecordAsync(
        AuthorityId authorityId,
        CorrespondenceTypeId correspondenceTypeId,
        RegulatoryApplicationId? regulatoryApplicationId,
        SubmissionId? submissionId,
        RegistrationId? registrationId,
        CancellationToken cancellationToken)
    {
        var authorityExists = await _dbContext.Authorities
            .AsNoTracking()
            .AnyAsync(x => x.Id == authorityId, cancellationToken);

        if (!authorityExists)
            throw new NotFoundException("The health authority was not found.");

        var typeExists = await _dbContext.CorrespondenceTypes
            .AsNoTracking()
            .AnyAsync(x => x.Id == correspondenceTypeId, cancellationToken);

        if (!typeExists)
            throw new NotFoundException("The correspondence type was not found.");

        // Each anchor is checked only when given. An unfiled letter is valid;
        // a letter filed against something the caller cannot see is not.
        if (regulatoryApplicationId is { } applicationId)
        {
            var exists = await _dbContext.RegulatoryApplications
                .AsNoTracking()
                .AnyAsync(x => x.Id == applicationId, cancellationToken);

            if (!exists)
                throw new NotFoundException("The regulatory application was not found.");
        }

        if (submissionId is { } submission)
        {
            var exists = await _dbContext.Submissions
                .AsNoTracking()
                .AnyAsync(x => x.Id == submission, cancellationToken);

            if (!exists)
                throw new NotFoundException("The submission was not found.");
        }

        if (registrationId is { } registration)
        {
            var exists = await _dbContext.Registrations
                .AsNoTracking()
                .AnyAsync(x => x.Id == registration, cancellationToken);

            if (!exists)
                throw new NotFoundException("The registration was not found.");
        }
    }
}
