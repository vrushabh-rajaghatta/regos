using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.SharedKernel.Exceptions;
using RegOS.Registration.Domain.Aggregates.Registration;

namespace RegOS.Registration.Application.Commands.AttachRegistrationToStep;

/// <summary>
/// Records that a registration was the outcome of a step of a plan.
/// </summary>
/// <remarks>
/// <b>It lives in Registration, not in Process</b> (ADR-065 D2, I2). The owning
/// aggregate sets its own foreign key; a Process command doing it would be
/// Process writing into a lifecycle that is not its own, and
/// <c>ContextDependencyTests</c> now refuses that shape in both directions.
/// <para>
/// <b>The step is checked for existence and nothing else.</b> There is no rule
/// that a plan's steps and the registrations attached to them concern the same
/// product — a plan may legitimately span work about several applications, and
/// inventing that invariant here would refuse something real. Existence is
/// checked because a dangling id is not an annotation, it is a typo.
/// </para>
/// </remarks>
public sealed class AttachRegistrationToStepHandler
{
    private readonly IRegistrationRepository _registrations;
    private readonly RegOSDbContext _dbContext;

    public AttachRegistrationToStepHandler(
        IRegistrationRepository registrations,
        RegOSDbContext dbContext)
    {
        _registrations = registrations;
        _dbContext = dbContext;
    }

    public async Task HandleAsync(
        AttachRegistrationToStepCommand command,
        CancellationToken cancellationToken)
    {
        var registration =
            await _registrations.GetByIdAsync(command.RegistrationId, cancellationToken)
            ?? throw new NotFoundException("That registration does not exist.");

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

        registration.AttachToStep(command.ProcessStepId);

        await _registrations.UpdateAsync(registration, cancellationToken);
    }
}
