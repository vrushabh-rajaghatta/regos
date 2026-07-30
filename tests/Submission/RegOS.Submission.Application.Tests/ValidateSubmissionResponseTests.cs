using FluentAssertions;

using RegOS.Submission.Application.Queries.ValidateSubmission;
using RegOS.Submission.Application.Validation.Models;

namespace RegOS.Submission.Application.Tests;

/// <summary>
/// Ordering is part of the contract, not a client's choice — a validation
/// screen is revisited after every change, so the sequence must not shuffle.
/// </summary>
public class ValidateSubmissionResponseTests
{
    [Fact]
    public void Issues_AreOrderedMostSevereFirst()
    {
        var result = new SubmissionValidationResult();
        result.AddIssue("B", "info", ValidationSeverity.Information);
        result.AddIssue("A", "error", ValidationSeverity.Error);
        result.AddIssue("C", "warning", ValidationSeverity.Warning);

        var response = ValidateSubmissionResponse.From(result);

        response.Issues.Select(i => i.Severity).Should().ContainInOrder(
            ValidationSeverity.Error,
            ValidationSeverity.Warning,
            ValidationSeverity.Information);
    }

    [Fact]
    public void WithinASeverity_IssuesAreOrderedByCodeThenRuleCodeThenMessage()
    {
        var result = new SubmissionValidationResult();
        result.AddIssue(new SubmissionValidationIssue(
            "BlueprintRuleViolation", "z", ValidationSeverity.Error, RuleCode: "R-2"));
        result.AddIssue(new SubmissionValidationIssue(
            "BlueprintRuleViolation", "a", ValidationSeverity.Error, RuleCode: "R-1"));
        result.AddIssue("A-Code", "first by code", ValidationSeverity.Error);

        var response = ValidateSubmissionResponse.From(result);

        response.Issues.Select(i => i.Message)
            .Should().ContainInOrder("first by code", "a", "z");
    }

    [Fact]
    public void TheSameIssuesAlwaysProduceTheSameOrder()
    {
        static SubmissionValidationResult Build()
        {
            var result = new SubmissionValidationResult();
            result.AddIssue("C", "three", ValidationSeverity.Warning);
            result.AddIssue("A", "one", ValidationSeverity.Error);
            result.AddIssue("B", "two", ValidationSeverity.Information);
            return result;
        }

        var first = ValidateSubmissionResponse.From(Build());
        var second = ValidateSubmissionResponse.From(Build());

        first.Issues.Select(i => i.Message)
            .Should().Equal(second.Issues.Select(i => i.Message));
    }

    [Fact]
    public void StructuredFields_SurviveProjection()
    {
        var result = new SubmissionValidationResult();
        result.AddIssue(new SubmissionValidationIssue(
            "BlueprintRulesNotEvaluated",
            "not evaluated",
            ValidationSeverity.Information,
            UnevaluatedRuleTypes: ["SectionNotEmpty"]));

        var issue = ValidateSubmissionResponse.From(result).Issues.Single();

        // Clients read the list, never the sentence.
        issue.UnevaluatedRuleTypes.Should().Equal("SectionNotEmpty");
    }
}
