namespace RegOS.Submission.Application.Queries.GetApplicationContacts;

/// <summary>
/// Who currently speaks for an application — <b>derived, never stored</b>
/// (ADR-048).
/// </summary>
/// <param name="AsOfSequenceNumber">
/// The sequence these were read from. Null when the application has published
/// nothing: before the first filing there is nobody named on a filing, and that
/// is an absence of a filing rather than missing data.
/// </param>
public sealed record ApplicationContacts(
    int? AsOfSequenceNumber,
    IReadOnlyList<ApplicationContact> Contacts);

public sealed record ApplicationContact(
    Guid ContactId,
    string ContactName,
    string? ContactTitle,
    string OrganizationName,
    Guid RoleId,
    string RoleName);
