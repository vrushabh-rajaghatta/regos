using FluentAssertions;

using RegOS.Submission.Application.Validation.Models;

namespace RegOS.Submission.Application.Tests;

/// <summary>
/// Readiness is derived from severity, not from the presence of issues. These
/// assert the model directly, independent of any validator and of the database:
/// the rule that decides whether a dossier may be published is worth pinning on
/// its own.
/// </summary>
public class SubmissionValidationResultTests
{
    [Fact]
    public void NoIssues_IsValid()
    {
        new SubmissionValidationResult().IsValid.Should().BeTrue();
    }

    [Fact]
    public void AnError_BlocksPublishing()
    {
        var result = new SubmissionValidationResult();

        result.AddIssue("X", "boom", ValidationSeverity.Error);

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(ValidationSeverity.Warning)]
    [InlineData(ValidationSeverity.Information)]
    public void NonBlockingIssues_DoNotBlockPublishing(ValidationSeverity severity)
    {
        var result = new SubmissionValidationResult();

        result.AddIssue("X", "advisory", severity);

        // Reported, but not a reason to stop.
        result.Issues.Should().ContainSingle();
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void OneErrorAmongAdvisories_StillBlocks()
    {
        var result = new SubmissionValidationResult();

        result.AddIssue("A", "note", ValidationSeverity.Information);
        result.AddIssue("B", "advice", ValidationSeverity.Warning);
        result.AddIssue("C", "boom", ValidationSeverity.Error);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void AddIssue_DefaultsToError()
    {
        var result = new SubmissionValidationResult();

        result.AddIssue("X", "boom");

        result.Issues.Single().Severity.Should().Be(ValidationSeverity.Error);
    }
}
