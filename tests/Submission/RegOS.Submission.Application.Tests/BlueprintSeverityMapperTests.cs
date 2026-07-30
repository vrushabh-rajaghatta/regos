using FluentAssertions;

using RegOS.Submission.Application.Validation;

using BlueprintSeverity = RegOS.ReferenceData.Domain.Blueprint.ValidationSeverity;
using IssueSeverity = RegOS.Submission.Application.Validation.Models.ValidationSeverity;

namespace RegOS.Submission.Application.Tests;

/// <summary>
/// The translation between how a blueprint grades a rule and how a failure
/// affects readiness. Pinned on its own because getting it wrong is silent: a
/// submission would publish that should have been stopped.
/// </summary>
public class BlueprintSeverityMapperTests
{
    [Fact]
    public void BlueprintError_Blocks()
    {
        BlueprintSeverityMapper.ToIssueSeverity(BlueprintSeverity.Error)
            .Should().Be(IssueSeverity.Error);
    }

    [Fact]
    public void BlueprintWarning_Advises()
    {
        BlueprintSeverityMapper.ToIssueSeverity(BlueprintSeverity.Warning)
            .Should().Be(IssueSeverity.Warning);
    }

    [Fact]
    public void UnknownSeverity_FailsClosed()
    {
        // A grading this engine does not recognise must not become the weakest
        // severity by default.
        BlueprintSeverityMapper.ToIssueSeverity((BlueprintSeverity)99)
            .Should().Be(IssueSeverity.Error);
    }

    [Fact]
    public void TheTwoEnumsDoNotShareOrdinals_SoCastingWouldBeWrong()
    {
        // The reason this mapper exists. Blueprint Error is 1 and issue
        // Information is 1, so a cast would downgrade a blocking regulatory
        // rule to a note. If this ever starts failing, the enums have been
        // realigned and someone will be tempted to delete the mapper — don't:
        // they are separate concepts in separate contexts.
        ((int)BlueprintSeverity.Error).Should().Be((int)IssueSeverity.Information);

        BlueprintSeverityMapper.ToIssueSeverity(BlueprintSeverity.Error)
            .Should().NotBe(IssueSeverity.Information);
    }
}
