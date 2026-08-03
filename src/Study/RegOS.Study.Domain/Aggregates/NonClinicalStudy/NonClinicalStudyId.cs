using RegOS.SharedKernel.Primitives;

namespace RegOS.Study.Domain.Aggregates.NonClinicalStudy;

public sealed class NonClinicalStudyId : StronglyTypedId
{
    public NonClinicalStudyId(Guid value) : base(value)
    {
    }

    public static NonClinicalStudyId New() => new(Guid.NewGuid());

    public static NonClinicalStudyId From(Guid value) => new(value);

    public static implicit operator Guid(NonClinicalStudyId id) => id.Value;
}
