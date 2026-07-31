using FluentAssertions;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

using RegistrationAggregate = RegOS.Registration.Domain.Aggregates.Registration.Registration;

namespace RegOS.Registration.Domain.Tests;

public class RegistrationTests
{
    private static readonly DateOnly Today = new(2026, 7, 31);

    private static RegistrationAggregate New(
        DateOnly? occurredOn = null,
        RegulatoryApplicationId? originatingApplicationId = null,
        string? note = null) =>
        RegistrationAggregate.Create(
            TenantId.New(),
            GlobalProductId.New(),
            new CountryId(Guid.NewGuid()),
            new AuthorityId(Guid.NewGuid()),
            new OrganizationId(Guid.NewGuid()),
            occurredOn ?? Today,
            originatingApplicationId,
            note);

    // --- Creation ------------------------------------------------------------

    [Fact]
    public void ANewRegistrationIsPlanned()
    {
        New().CurrentStatus.Should().Be(RegistrationStatus.Planned);
    }

    [Fact]
    public void ANewRegistrationHoldsNoNumberOrDates()
    {
        var registration = New();

        registration.RegistrationNumber.Should().BeNull();
        registration.ApprovedOn.Should().BeNull();
        registration.ExpiresOn.Should().BeNull();
    }

    /// <summary>
    /// The first history entry is the status it starts in, not a separate
    /// "created" event: the history is one chronological sequence of states, in
    /// one vocabulary.
    /// </summary>
    [Fact]
    public void CreationIsRecordedAsTheFirstStatusInHistory()
    {
        var registration = New();

        var entry = registration.History.Should().ContainSingle().Subject;

        entry.Status.Should().Be(RegistrationStatus.Planned);
        entry.OccurredOn.Should().Be(Today);
    }

    /// <summary>
    /// The provenance guarantee: a portfolio migrated from a legacy register
    /// must be able to say when things happened, not merely when they were
    /// typed in.
    /// </summary>
    [Fact]
    public void HistoryDistinguishesWhenItHappenedFromWhenRegOSLearned()
    {
        var granted = new DateOnly(2019, 4, 12);

        var registration = New(occurredOn: granted);

        var entry = registration.History.Single();

        entry.OccurredOn.Should().Be(granted);
        entry.RecordedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void TheOriginatingApplicationIsOptional()
    {
        New().OriginatingApplicationId.Should().BeNull();
    }

    [Fact]
    public void TheOriginatingApplicationIsKeptWhenSupplied()
    {
        var applicationId = new RegulatoryApplicationId(Guid.NewGuid());

        New(originatingApplicationId: applicationId)
            .OriginatingApplicationId.Should().Be(applicationId);
    }

    [Fact]
    public void ADateIsRequired()
    {
        var create = () => New(occurredOn: default(DateOnly));

        create.Should().Throw<DomainException>()
            .WithMessage(RegistrationErrors.OccurredOnRequired);
    }

    [Fact]
    public void AProductIsRequired()
    {
        var create = () => RegistrationAggregate.Create(
            TenantId.New(),
            default,
            new CountryId(Guid.NewGuid()),
            new AuthorityId(Guid.NewGuid()),
            new OrganizationId(Guid.NewGuid()),
            Today);

        create.Should().Throw<DomainException>()
            .WithMessage(RegistrationErrors.ProductRequired);
    }

    [Fact]
    public void AHolderIsRequired()
    {
        var create = () => RegistrationAggregate.Create(
            TenantId.New(),
            GlobalProductId.New(),
            new CountryId(Guid.NewGuid()),
            new AuthorityId(Guid.NewGuid()),
            default,
            Today);

        create.Should().Throw<DomainException>()
            .WithMessage(RegistrationErrors.HolderOrganizationRequired);
    }

    // --- Recording the grant -------------------------------------------------

    [Fact]
    public void RecordingApprovalSetsTheNumberDatesAndStatus()
    {
        var registration = New(occurredOn: new DateOnly(2026, 1, 1));
        var approved = new DateOnly(2026, 3, 1);
        var expires = new DateOnly(2031, 3, 1);

        registration.RecordApproval("NDA-123456", approved, expires);

        registration.CurrentStatus.Should().Be(RegistrationStatus.Approved);
        registration.RegistrationNumber.Should().Be("NDA-123456");
        registration.ApprovedOn.Should().Be(approved);
        registration.ExpiresOn.Should().Be(expires);
    }

    /// <summary>
    /// The whole reason creation cannot start Approved: an authorisation
    /// granted in 2019 and entered today must record 2019, and the clock is
    /// never consulted for a business date.
    /// </summary>
    /// <remarks>
    /// The complete migration story: both entries carry their 2019 business
    /// dates, in the order they happened, while both were recorded today.
    /// </remarks>
    [Fact]
    public void ApprovalUsesTheBusinessDateNotTheClock()
    {
        var registration = New(occurredOn: new DateOnly(2019, 1, 15));
        var granted = new DateOnly(2019, 4, 12);

        registration.RecordApproval("EU/1/19/1234", granted);

        registration.ApprovedOn.Should().Be(granted);

        var entry = registration.History
            .Single(h => h.Status == RegistrationStatus.Approved);

        entry.OccurredOn.Should().Be(granted);
        entry.RecordedOnUtc.Date.Should().Be(DateTime.UtcNow.Date);
    }

    [Fact]
    public void ApprovalAppendsToHistoryRatherThanReplacingIt()
    {
        var registration = New(occurredOn: new DateOnly(2026, 1, 1));

        registration.RecordApproval("NDA-1", new DateOnly(2026, 3, 1));

        registration.History.Should().HaveCount(2);
        registration.History.Select(h => h.Status).Should().ContainInOrder(
            RegistrationStatus.Planned, RegistrationStatus.Approved);
    }

    [Fact]
    public void ANumberIsRequiredToRecordApproval()
    {
        var registration = New();

        var record = () => registration.RecordApproval("  ", Today);

        record.Should().Throw<DomainException>()
            .WithMessage(RegistrationErrors.RegistrationNumberRequired);
    }

    [Fact]
    public void TheNumberIsTrimmed()
    {
        var registration = New();

        registration.RecordApproval("  NDA-9  ", Today);

        registration.RegistrationNumber.Should().Be("NDA-9");
    }

    [Fact]
    public void ARegistrationCannotExpireBeforeItWasApproved()
    {
        var registration = New();

        var record = () => registration.RecordApproval(
            "NDA-1", new DateOnly(2026, 3, 1), new DateOnly(2025, 3, 1));

        record.Should().Throw<DomainException>()
            .WithMessage(RegistrationErrors.ExpiryBeforeApproval);
    }

    [Fact]
    public void ExpiryOnTheApprovalDateIsAllowed()
    {
        var registration = New(occurredOn: new DateOnly(2026, 1, 1));
        var sameDay = new DateOnly(2026, 3, 1);

        var record = () => registration.RecordApproval("NDA-1", sameDay, sameDay);

        record.Should().NotThrow();
    }

    [Fact]
    public void ApprovalCannotBeRecordedTwice()
    {
        var registration = New();
        registration.RecordApproval("NDA-1", Today);

        var again = () => registration.RecordApproval("NDA-2", Today);

        again.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(RegistrationErrors.ApprovalAlreadyRecorded);
    }

    [Fact]
    public void AFailedApprovalLeavesTheRegistrationUntouched()
    {
        var registration = New();

        var record = () => registration.RecordApproval("", Today);
        record.Should().Throw<DomainException>();

        registration.CurrentStatus.Should().Be(RegistrationStatus.Planned);
        registration.History.Should().ContainSingle();
    }

    // --- Notes ---------------------------------------------------------------

    [Fact]
    public void ANoteIsKeptOnTheHistoryEntry()
    {
        var registration = New(note: "Carried over from the legacy register.");

        registration.History.Single().Note
            .Should().Be("Carried over from the legacy register.");
    }

    [Fact]
    public void AnEmptyNoteIsStoredAsNull()
    {
        New(note: "   ").History.Single().Note.Should().BeNull();
    }

    [Fact]
    public void AnOverlongNoteIsRejected()
    {
        var create = () => New(note: new string('x', 501));

        create.Should().Throw<DomainException>()
            .WithMessage(RegistrationErrors.NoteTooLong);
    }
}
