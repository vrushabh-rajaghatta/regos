namespace RegOS.Api.Endpoints.Inspections;

/// <param name="InitialStatus">
/// <c>Announced</c> when they told us in advance, <c>InProgress</c> when they
/// arrived unannounced.
/// </param>
/// <param name="OrganizationSiteId">
/// What was inspected. Optional: "the FDA will inspect us in March" arrives
/// before anyone knows which plant.
/// </param>
public sealed record BeginInspectionRequest(
    Guid AuthorityId,
    string Title,
    string InitialStatus,
    DateOnly OccurredOn,
    Guid? OrganizationSiteId = null,
    DateOnly? ScheduledFor = null);
