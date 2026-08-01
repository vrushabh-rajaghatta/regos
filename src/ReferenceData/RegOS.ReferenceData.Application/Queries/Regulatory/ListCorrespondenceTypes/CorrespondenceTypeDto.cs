namespace RegOS.ReferenceData.Application.Queries.Regulatory.ListCorrespondenceTypes;

public sealed record CorrespondenceTypeDto(
    Guid Id,
    string Code,
    string Name);
