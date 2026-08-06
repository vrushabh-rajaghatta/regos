using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.SharedKernel.Exceptions;
using RegOS.Submission.Domain.Submission;

namespace RegOS.Submission.Application.Commands.AttachSubmissionToStep;

/// <summary>
/// Records that a submission contributes to a step of a plan.
/// </summary>
/// <remarks>
/// <b>It lives in Submission, not in Process</b> (ADR-065 D2, I2). The owning
/// aggregate sets its own foreign key; a Process command doing it would be
/// Process writing into a lifecycle that is not its own, and
/// <c>ContextDependencyTests</c> now refuses that shape in both directions.
/// <para>
/// <b>The step is checked for existence and nothing else.</b> There is no rule
/// that a plan's steps and the submissions attached to them concern the same
/// product — a plan may legitimately span work about several applications, and
/// inventing that invariant here would refuse something real. Existence is
/// checked because a dangling id is not an annotation, it is a typo.
/// </para>
/// </remarks>
public sealed class AttachSubmissionToStepHandler
{
    private readonly ISubmissionRepository _submissions;
    private readonly RegOSDbContext _dbContext;

    public AttachSubmissionToStepHandler(
        ISubmissionRepository submissions,
        RegOSDbContext dbContext)
    {
        _submissions = submissions;
        _dbContext = dbContext;
    }

    public async Task HandleAsync(
        AttachSubmissionToStepCommand command,
        CancellationToken cancellationToken)
    {
        var submission =
            await _submissions.GetByIdAsync(command.SubmissionId, cancellationToken)
            ?? throw new NotFoundException("That submission does not exist.");

        if (command.ProcessStepId is { } stepId)
        {
            // A read over the context, never the Process repository — ADR-016
            // grants the read; the repository rule closes the write.
            var stepExists = await _dbContext.ProcessPlans
                .AsNoTracking()
                .AnyAsync(plan => plan.Steps.Any(s => s.Id == stepId), cancellationToken);

            if (!stepExists)
                throw new NotFoundException("That plan step does not exist.");
        }

        submission.AttachToStep(command.ProcessStepId);

        await _submissions.UpdateAsync(submission, cancellationToken);
    }
}
