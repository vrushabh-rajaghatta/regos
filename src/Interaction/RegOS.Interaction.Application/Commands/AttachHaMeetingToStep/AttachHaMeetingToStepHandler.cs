using Microsoft.EntityFrameworkCore;

using RegOS.Interaction.Domain.Meetings;
using RegOS.Persistence;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Interaction.Application.Commands.AttachHaMeetingToStep;

/// <summary>
/// Records which planned work a meeting serves.
/// </summary>
/// <remarks>
/// <b>It lives in Interaction, not in Process</b> (ADR-065 D2, I2). The owning
/// aggregate sets its own foreign key; a Process command doing it would be
/// Process writing into a lifecycle that is not its own, and
/// <c>ContextDependencyTests</c> refuses that shape in both directions.
/// <para>
/// <b>The step is checked for existence and nothing else.</b> There is no rule
/// that a plan and the interactions attached to its steps concern the same
/// product or application — a conversation with an authority can be about
/// anything RegOS holds, which is why this context depends on the most. Existence
/// is checked because a dangling id is not an annotation, it is a typo.
/// </para>
/// </remarks>
public sealed class AttachHaMeetingToStepHandler
{
    private readonly IHaMeetingRepository _meetings;
    private readonly RegOSDbContext _dbContext;

    public AttachHaMeetingToStepHandler(
        IHaMeetingRepository meetings,
        RegOSDbContext dbContext)
    {
        _meetings = meetings;
        _dbContext = dbContext;
    }

    public async Task HandleAsync(
        AttachHaMeetingToStepCommand command,
        CancellationToken cancellationToken)
    {
        var meeting =
            await _meetings.GetByIdAsync(command.HaMeetingId, cancellationToken)
            ?? throw new NotFoundException("That meeting does not exist.");

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

        meeting.AttachToStep(command.ProcessStepId);

        await _meetings.UpdateAsync(meeting, cancellationToken);
    }
}
