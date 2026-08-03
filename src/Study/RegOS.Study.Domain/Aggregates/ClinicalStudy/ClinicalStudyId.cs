using RegOS.SharedKernel.Primitives;

namespace RegOS.Study.Domain.Aggregates.ClinicalStudy;

public sealed class ClinicalStudyId : StronglyTypedId
{
    public ClinicalStudyId(Guid value) : base(value)
    {
    }

    public static ClinicalStudyId New() => new(Guid.NewGuid());

    public static ClinicalStudyId From(Guid value) => new(value);

    public static implicit operator Guid(ClinicalStudyId id) => id.Value;
}
