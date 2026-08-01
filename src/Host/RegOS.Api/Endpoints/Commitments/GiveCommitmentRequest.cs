namespace RegOS.Api.Endpoints.Commitments;

/// <param name="GivenOn">
/// When we made the promise. Supplied, never taken from the clock — a
/// post-marketing commitment carried over from an approval letter is usually
/// years old.
/// </param>
public sealed record GiveCommitmentRequest(
    Guid AuthorityId,
    string Title,
    DateOnly GivenOn,
    DateOnly DueOn,
    string? Description = null,
    Guid? OwnerUserId = null,
    Guid? RegistrationId = null,
    Guid? RegulatoryApplicationId = null,
    Guid? SourceCorrespondenceId = null);
