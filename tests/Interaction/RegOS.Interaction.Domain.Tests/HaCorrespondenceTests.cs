using FluentAssertions;

using RegOS.Interaction.Domain.Correspondence;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.ReferenceData.Domain.Regulatory.Correspondence;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Interaction.Domain.Tests;

public sealed class HaCorrespondenceTests
{
    private static readonly TenantId Tenant = TenantId.New();
    private static readonly AuthorityId Authority = new(Guid.NewGuid());
    private static readonly CorrespondenceTypeId Type = new(Guid.NewGuid());
    private static readonly DateOnly Dated = new(2026, 3, 1);

    private static HaCorrespondence Record(
        DateOnly? occurredOn = null,
        DateOnly? responseDueOn = null,
        CorrespondenceDirection direction = CorrespondenceDirection.Inbound,
        string subject = "Information request on IND 123456",
        string? authorityReference = null,
        AuthorityDivisionId? authorityDivisionId = null)
        => HaCorrespondence.Record(
            Tenant,
            Authority,
            Type,
            authorityDivisionId,
            direction,
            subject,
            occurredOn ?? Dated,
            responseDueOn,
            authorityReference);

    [Fact]
    public void ARecordedLetterKeepsTheDateItWasWrittenAndTheDateWeLearnedOfIt()
    {
        var before = DateTime.UtcNow;

        var letter = Record(occurredOn: new DateOnly(2019, 6, 14));

        letter.OccurredOn.Should().Be(new DateOnly(2019, 6, 14));
        letter.RecordedOnUtc.Should().BeOnOrAfter(before);

        // The whole point of keeping both: a letter logged today may be seven
        // years old, and neither date is a substitute for the other.
        letter.OccurredOn.Year.Should().NotBe(letter.RecordedOnUtc.Year);
    }

    [Fact]
    public void ALetterHasNoStatus()
    {
        var letter = Record();

        // Deliberately asserted as an absence (ADR-040 decision 4). A letter is
        // an event: it does not change once it has happened. If someone adds a
        // status property this test is where the conversation starts.
        letter.GetType()
            .GetProperties()
            .Select(p => p.Name)
            .Should()
            .NotContain(name => name.Contains("Status", StringComparison.Ordinal));
    }

    [Fact]
    public void AResponseCannotBeDueBeforeTheLetterItAnswers()
    {
        var act = () => Record(
            occurredOn: new DateOnly(2026, 3, 1),
            responseDueOn: new DateOnly(2026, 2, 28));

        act.Should()
            .Throw<DomainException>()
            .WithMessage(HaCorrespondenceErrors.ResponseDueBeforeOccurred);
    }

    [Fact]
    public void AResponseMayBeDueOnOutboundCorrespondence()
    {
        // We send a meeting request; the authority owes us an answer by a date.
        // Who owes the response is derived from direction, never enforced here.
        var letter = Record(
            direction: CorrespondenceDirection.Outbound,
            responseDueOn: new DateOnly(2026, 3, 30));

        letter.Direction.Should().Be(CorrespondenceDirection.Outbound);
        letter.ResponseDueOn.Should().Be(new DateOnly(2026, 3, 30));
    }

    [Fact]
    public void ALetterNeedsAnAuthorityAndAType()
    {
        var withoutAuthority = () => HaCorrespondence.Record(
            Tenant, default, Type, null, CorrespondenceDirection.Inbound, "x", Dated);

        var withoutType = () => HaCorrespondence.Record(
            Tenant, Authority, default, null, CorrespondenceDirection.Inbound, "x", Dated);

        withoutAuthority.Should().Throw<DomainException>()
            .WithMessage(HaCorrespondenceErrors.AuthorityRequired);

        withoutType.Should().Throw<DomainException>()
            .WithMessage(HaCorrespondenceErrors.CorrespondenceTypeRequired);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ALetterNeedsASubject(string subject)
    {
        var act = () => Record(subject: subject);

        act.Should().Throw<DomainException>()
            .WithMessage(HaCorrespondenceErrors.SubjectRequired);
    }

    [Fact]
    public void ASubjectIsTrimmedAndBounded()
    {
        Record(subject: "  Deficiency letter  ").Subject
            .Should().Be("Deficiency letter");

        var tooLong = () => Record(
            subject: new string('x', HaCorrespondence.SubjectMaxLength + 1));

        tooLong.Should().Throw<DomainException>()
            .WithMessage(HaCorrespondenceErrors.SubjectTooLong);
    }

    [Fact]
    public void AnEmptyAuthorityReferenceIsNullRatherThanBlank()
    {
        Record(authorityReference: "   ").AuthorityReference.Should().BeNull();
        Record(authorityReference: " IND 123456 ").AuthorityReference
            .Should().Be("IND 123456");
    }

    [Fact]
    public void ADivisionIsOptional_AndTheAggregateDoesNotPolicyItsOwner()
    {
        var division = new AuthorityDivisionId(Guid.NewGuid());

        Record().AuthorityDivisionId.Should().BeNull();
        Record(authorityDivisionId: division).AuthorityDivisionId
            .Should().Be(division);

        // Deliberately no check here that the division belongs to the
        // authority: that is another aggregate's fact, and this one cannot see
        // its rows. The rule lives in IHaCorrespondencePolicy — stated
        // generally, as "a referenced child must belong to its selected
        // parent", so a committee or an office inherits it unchanged.
    }

    [Fact]
    public void AmendingCanClearOrChangeTheDivision()
    {
        var division = new AuthorityDivisionId(Guid.NewGuid());
        var letter = Record(authorityDivisionId: division);

        letter.Amend(Type, null, "Same subject", Dated, null, null);

        // The letter was re-read and the division turned out to be wrong.
        // Clearing it is a correction, not a deletion of history.
        letter.AuthorityDivisionId.Should().BeNull();
    }

    [Fact]
    public void ALetterCanBeFiledAgainstNothing()
    {
        var letter = Record();

        // A guidance notification concerns no application. Requiring an anchor
        // would make users invent one, which is worse than a null.
        letter.RegulatoryApplicationId.Should().BeNull();
        letter.SubmissionId.Should().BeNull();
        letter.RegistrationId.Should().BeNull();
    }

    [Fact]
    public void FilingAgainstSomethingSetsAllThreeAnchorsTogether()
    {
        var letter = Record();
        var application = new RegulatoryApplicationId(Guid.NewGuid());

        letter.FileAgainst(application, null, null);

        letter.RegulatoryApplicationId.Should().Be(application);
        letter.SubmissionId.Should().BeNull();

        // Re-filing replaces the whole answer to "what is this about?" rather
        // than accumulating anchors nobody chose.
        var registration = new RegistrationId(Guid.NewGuid());
        letter.FileAgainst(null, null, registration);

        letter.RegulatoryApplicationId.Should().BeNull();
        letter.RegistrationId.Should().Be(registration);
    }

    [Fact]
    public void AmendingCorrectsWhatWasTypedButNotWhichLetterItIs()
    {
        var letter = Record();
        var originalAuthority = letter.AuthorityId;
        var originalDirection = letter.Direction;

        letter.Amend(
            Type,
            null,
            "Corrected subject",
            new DateOnly(2026, 3, 2),
            new DateOnly(2026, 4, 1),
            "IND 999");

        letter.Subject.Should().Be("Corrected subject");
        letter.OccurredOn.Should().Be(new DateOnly(2026, 3, 2));
        letter.ResponseDueOn.Should().Be(new DateOnly(2026, 4, 1));
        letter.AuthorityReference.Should().Be("IND 999");

        // Not amendable: getting these wrong means the wrong letter was
        // logged, and that is a different correction.
        letter.AuthorityId.Should().Be(originalAuthority);
        letter.Direction.Should().Be(originalDirection);
    }

    [Fact]
    public void AmendingStillRefusesAResponseDueBeforeTheLetter()
    {
        var letter = Record();

        var act = () => letter.Amend(
            Type, null, "x", new DateOnly(2026, 3, 1), new DateOnly(2026, 2, 1), null);

        act.Should().Throw<DomainException>()
            .WithMessage(HaCorrespondenceErrors.ResponseDueBeforeOccurred);
    }
}
