namespace RegOS.ReferenceData.Application.Queries.SubmissionTypes.ListSubmissionTypes;

public sealed record SubmissionTypeDto(
    Guid Id,
    string Code,
    string Name,
    Guid AuthorityId);
