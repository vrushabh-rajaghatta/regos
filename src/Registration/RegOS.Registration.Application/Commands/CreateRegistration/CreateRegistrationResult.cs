using RegOS.Registration.Domain.Aggregates.Registration;

namespace RegOS.Registration.Application.Commands.CreateRegistration;

public sealed record CreateRegistrationResult(RegistrationId Id);
