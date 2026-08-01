using RegOS.Platform.Contracts;

namespace RegOS.Interaction.Application.Queries.ListDueWork;

/// <summary>
/// <em>"What do I need to work on today?"</em>
/// </summary>
/// <param name="OwnerUserId">
/// Resolved by the endpoint from the authenticated identity when the caller
/// asks for their own work. The handler takes an id rather than reading the
/// current user, so this context depends on no Platform service.
/// </param>
/// <param name="DueOnOrBefore">
/// Optional horizon — "this week" is a date the caller computes, not a word the
/// server interprets.
/// </param>
public sealed record ListDueWorkQuery(
    UserId? OwnerUserId = null,
    DateOnly? DueOnOrBefore = null);
