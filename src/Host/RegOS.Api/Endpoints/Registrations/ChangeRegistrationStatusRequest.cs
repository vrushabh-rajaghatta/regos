using RegOS.Registration.Domain.Aggregates.Registration;

namespace RegOS.Api.Endpoints.Registrations;

/// <param name="OccurredOn">The business date the new status took effect.</param>
public sealed record ChangeRegistrationStatusRequest(
    RegistrationStatus Status,
    DateOnly OccurredOn,
    string? Note = null);
