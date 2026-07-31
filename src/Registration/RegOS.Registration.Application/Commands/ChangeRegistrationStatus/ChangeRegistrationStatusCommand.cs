using RegOS.Registration.Domain.Aggregates.Registration;

namespace RegOS.Registration.Application.Commands.ChangeRegistrationStatus;

/// <param name="OccurredOn">
/// The business date the new status took effect — never the clock, so a
/// suspension imposed last month records last month.
/// </param>
public sealed record ChangeRegistrationStatusCommand(
    RegistrationId RegistrationId,
    RegistrationStatus Status,
    DateOnly OccurredOn,
    string? Note = null);
