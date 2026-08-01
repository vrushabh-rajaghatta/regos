using FluentAssertions;

using RegOS.Interaction.Domain.Correspondence;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.Regulatory.Correspondence;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Interaction.Domain.Tests;

public sealed class HaQuestionTests
{
    private static readonly DateOnly Dated = new(2026, 3, 1);

    private static HaCorrespondence Letter()
        => HaCorrespondence.Record(
            TenantId.New(),
            new AuthorityId(Guid.NewGuid()),
            new CorrespondenceTypeId(Guid.NewGuid()),
            null,
            CorrespondenceDirection.Inbound,
            "Information request",
            Dated);

    [Fact]
    public void AQuestionStartsOpenOnTheDateOfTheLetterItArrivedIn()
    {
        var letter = Letter();

        var question = letter.RaiseQuestion("3a", "Justify the stability data.");

        question.CurrentStatus.Should().Be(HaQuestionStatus.Open);
        question.History.Should().ContainSingle();
        question.History[0].OccurredOn.Should().Be(Dated);
        question.RespondedOn.Should().BeNull();
    }

    [Fact]
    public void TwoQuestionsCannotShareANumberWithinOneLetter()
    {
        var letter = Letter();
        letter.RaiseQuestion("3", "First");

        var act = () => letter.RaiseQuestion("3", "Second");

        // Two questions numbered 3 in one letter is a transcription error, not
        // a business case.
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(HaCorrespondenceErrors.QuestionNumberNotUnique);
    }

    [Fact]
    public void RespondingRecordsTheAnswerAndDerivesTheDateFromTheHistory()
    {
        var letter = Letter();
        var question = letter.RaiseQuestion("1", "Clarify the assay.");

        letter.RespondToQuestion(
            question.Id, "See section 3.2.P.5.", new DateOnly(2026, 4, 10));

        question.CurrentStatus.Should().Be(HaQuestionStatus.Responded);
        question.ResponseText.Should().Be("See section 3.2.P.5.");

        // Derived, never stored — a second copy of a fact the history already
        // holds could disagree with it.
        question.RespondedOn.Should().Be(new DateOnly(2026, 4, 10));
    }

    [Fact]
    public void RespondedAndResolvedAreDifferentQuestionsWithDifferentActors()
    {
        var letter = Letter();
        var question = letter.RaiseQuestion("1", "Clarify the assay.");

        letter.RespondToQuestion(question.Id, "Answer", new DateOnly(2026, 4, 10));
        question.CurrentStatus.Should().Be(HaQuestionStatus.Responded);

        // Weeks pass. "Have we answered?" and "has the authority accepted it?"
        // are not the same question, and the gap between them is exactly the
        // period a regulatory team is anxious about.
        letter.ResolveQuestion(question.Id, new DateOnly(2026, 5, 30));

        question.CurrentStatus.Should().Be(HaQuestionStatus.Resolved);
        question.RespondedOn.Should().Be(new DateOnly(2026, 4, 10));
        question.History.Should().HaveCount(3);
    }

    [Fact]
    public void AQuestionsHistoryCannotGoBackwards()
    {
        var letter = Letter();
        var question = letter.RaiseQuestion("1", "Clarify the assay.");

        var act = () => letter.RespondToQuestion(
            question.Id, "Answer", new DateOnly(2026, 2, 1));

        act.Should().Throw<DomainException>()
            .WithMessage(HaCorrespondenceErrors.QuestionHistoryOutOfOrder);
    }

    [Fact]
    public void AResolvedQuestionIsTerminal()
    {
        var letter = Letter();
        var question = letter.RaiseQuestion("1", "Clarify the assay.");

        letter.RespondToQuestion(question.Id, "Answer", new DateOnly(2026, 4, 10));
        letter.ResolveQuestion(question.Id, new DateOnly(2026, 5, 30));

        var respondAgain = () => letter.RespondToQuestion(
            question.Id, "More", new DateOnly(2026, 6, 1));

        // A resolved question that reopens is a new question in a new letter,
        // which is how authorities actually do it.
        respondAgain.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(HaCorrespondenceErrors.QuestionAlreadyResolved);
    }

    [Fact]
    public void AQuestionFromAnotherLetterIsNotReachable()
    {
        var ours = Letter();
        var theirs = Letter();
        var theirQuestion = theirs.RaiseQuestion("1", "Theirs");

        var act = () => ours.ResolveQuestion(theirQuestion.Id, Dated);

        act.Should().Throw<NotFoundException>()
            .WithMessage(HaCorrespondenceErrors.QuestionNotFound);
    }

    [Fact]
    public void TheTargetDateIsOursAndTheLettersDueDateIsTheirs()
    {
        var letter = HaCorrespondence.Record(
            TenantId.New(),
            new AuthorityId(Guid.NewGuid()),
            new CorrespondenceTypeId(Guid.NewGuid()),
            null,
            CorrespondenceDirection.Inbound,
            "Information request",
            Dated,
            responseDueOn: new DateOnly(2026, 4, 30));

        var question = letter.RaiseQuestion(
            "1", "Clarify.", targetResponseOn: new DateOnly(2026, 4, 15));

        // Two clocks, deliberately never one word: the letter's is the
        // regulator's deadline, the question's is our internal plan, and the
        // "what's due" view shows both at once.
        letter.ResponseDueOn.Should().Be(new DateOnly(2026, 4, 30));
        question.TargetResponseOn.Should().Be(new DateOnly(2026, 4, 15));
    }
}
