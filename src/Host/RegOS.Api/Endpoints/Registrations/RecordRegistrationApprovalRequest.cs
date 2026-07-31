namespace RegOS.Api.Endpoints.Registrations;

/// <param name="ApprovedOn">The date the authority granted it.</param>
public sealed record RecordRegistrationApprovalRequest(
    string RegistrationNumber,
    DateOnly ApprovedOn,
    DateOnly? ExpiresOn = null,
    string? Note = null);
