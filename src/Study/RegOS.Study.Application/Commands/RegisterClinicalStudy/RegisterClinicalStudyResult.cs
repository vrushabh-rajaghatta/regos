using RegOS.Study.Domain.Aggregates.ClinicalStudy;

namespace RegOS.Study.Application.Commands.RegisterClinicalStudy;

public sealed record RegisterClinicalStudyResult(ClinicalStudyId Id);
