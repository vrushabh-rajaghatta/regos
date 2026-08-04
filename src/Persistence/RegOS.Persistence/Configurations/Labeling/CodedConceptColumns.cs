using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.ReferenceData.Domain.Terminology;

namespace RegOS.Persistence.Configurations.Labeling;

/// <summary>
/// The three columns a coded value occupies, parameterised only by prefix.
/// </summary>
/// <remarks>
/// Moved out of <c>IndicationConfiguration</c> in S004, when its callers
/// stopped being one aggregate's. Persistence mechanics only — there is no
/// domain type behind it (ADR-059 §4).
/// </remarks>
internal static class CodedConceptColumns
{
    internal static Action<OwnedNavigationBuilder<TOwner, CodedConcept>> Of<TOwner>(
        string prefix,
        bool required = true)
        where TOwner : class
        => concept =>
        {
            var system = concept.Property(x => x.System)
                .HasColumnName($"{prefix}System")
                .HasMaxLength(CodedConcept.SystemMaxLength);

            var code = concept.Property(x => x.Code)
                .HasColumnName($"{prefix}Code")
                .HasMaxLength(CodedConcept.CodeMaxLength);

            var display = concept.Property(x => x.Display)
                .HasColumnName($"{prefix}Display")
                .HasMaxLength(CodedConcept.DisplayMaxLength);

            if (!required) return;

            system.IsRequired();
            code.IsRequired();
            display.IsRequired();
        };
}
