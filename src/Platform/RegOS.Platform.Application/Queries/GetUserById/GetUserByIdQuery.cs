using RegOS.Platform.Domain.Aggregates.User;
using RegOS.Platform.Contracts;

namespace RegOS.Platform.Application.Queries.GetUserById;

/// <summary>
/// Reads a single user within the caller's tenant. A user belonging to another
/// tenant is reported as not found, never as forbidden.
/// </summary>
public sealed record GetUserByIdQuery(UserId UserId);
