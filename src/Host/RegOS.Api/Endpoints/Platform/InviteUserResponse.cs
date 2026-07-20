using RegOS.Platform.Domain.Aggregates.User;

namespace RegOS.Api.Endpoints.Platform;

public sealed record InviteUserResponse(
    Guid Id,
    UserStatus Status);
