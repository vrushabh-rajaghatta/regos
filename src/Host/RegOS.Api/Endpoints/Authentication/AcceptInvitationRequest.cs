namespace RegOS.Api.Endpoints.Authentication;

public sealed record AcceptInvitationRequest(string? Token, string? Password);
