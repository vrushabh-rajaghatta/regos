namespace RegOS.ReferenceData.Application.Queries.SubmissionTypes.ListSubmissionTypes;

/// <param name="Token">
/// The eCTD wire value, or null when it is not in evidence. <b>Exposed on
/// purpose</b>: a screen that offers a choice which cannot be rendered should be
/// able to say so before the user makes it, rather than failing at package time.
/// </param>
public sealed record SubmissionTypeDto(
    Guid Id,
    string Code,
    string Name,
    string? Token,
    Guid AuthorityId);
