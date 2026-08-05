using FluentAssertions;

using RegOS.Product.Domain.Product;
using RegOS.Registration.Domain.Aggregates.PackAuthorisations;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Registration.Domain.Tests;

/// <summary>
/// A licence authorising one pack, from a date.
/// </summary>
/// <remarks>
/// <b>The shape the dependency graph forced, and the model wanted anyway</b>
/// (ADR-061 §3). `Registration.Domain → Product.Domain` already exists, so a
/// `RegistrationId` on the pack would have closed a cycle. Moving the
/// relationship here left `Registration` untouched, kept `Product` independent
/// of who authorised anything, and gave the relationship a date a foreign key
/// could never carry.
/// </remarks>
public sealed class PackAuthorisationTests
{
    private static readonly TenantId Tenant = TenantId.New();

    private static PackAuthorisation An(
        RegistrationId? registration = null,
        PackagedProductId? pack = null,
        DateOnly? on = null)
        => PackAuthorisation.Create(
            Tenant,
            registration ?? RegistrationId.New(),
            pack ?? PackagedProductId.New(),
            on ?? new DateOnly(2024, 3, 1));

    /// <summary>
    /// <b>The fact a foreign key could not carry.</b> A licence granted in 2021
    /// that gained its 100-pack in 2024 by variation has two dates, and only one
    /// of them is the registration's.
    /// </summary>
    [Fact]
    public void TheAuthorisationCarriesItsOwnDate()
    {
        An(on: new DateOnly(2024, 3, 1))
            .AuthorisedOn.Should().Be(new DateOnly(2024, 3, 1));
    }

    [Fact]
    public void AnAuthorisationWithNoDateIsRefused()
    {
        var create = () => PackAuthorisation.Create(
            Tenant, RegistrationId.New(), PackagedProductId.New(), default);

        create.Should().Throw<DomainException>()
            .WithMessage(PackAuthorisationErrors.AuthorisedOnRequired);
    }

    [Fact]
    public void AnAuthorisationOfNoPackIsRefused()
    {
        var create = () => PackAuthorisation.Create(
            Tenant, RegistrationId.New(), null!, new DateOnly(2024, 3, 1));

        create.Should().Throw<DomainException>()
            .WithMessage(PackAuthorisationErrors.PackRequired);
    }

    /// <summary>
    /// <b>One licence, many packs</b> — RIM says <c>License → Packaged
    /// Product</c>, <em>Single</em>, and that is wrong: an authorisation
    /// routinely covers a family of pack sizes.
    /// </summary>
    [Fact]
    public void OneLicenceMayAuthoriseSeveralPacks()
    {
        var licence = RegistrationId.New();

        var thirty = An(licence, PackagedProductId.New());
        var hundred = An(licence, PackagedProductId.New());

        thirty.RegistrationId.Should().Be(hundred.RegistrationId);
        thirty.Id.Should().NotBe(hundred.Id);
    }

    /// <summary>
    /// And the other way: a partial divestment leaves one pack authorised under
    /// two licences, which is why nothing is unique on the pack alone.
    /// </summary>
    [Fact]
    public void OnePackMayBeAuthorisedUnderTwoLicences()
    {
        var pack = PackagedProductId.New();

        An(RegistrationId.New(), pack).PackagedProductId
            .Should().Be(An(RegistrationId.New(), pack).PackagedProductId);
    }

    /// <summary>
    /// The date is correctable; the pair it names is not. A different pack or a
    /// different licence is a different authorisation, and editing one into
    /// another would leave no way to tell a correction from a replacement.
    /// </summary>
    [Fact]
    public void TheDateIsCorrectableAndThePairIsNot()
    {
        var authorisation = An(on: new DateOnly(2024, 3, 1));

        authorisation.Correct(new DateOnly(2024, 4, 15));

        authorisation.AuthorisedOn.Should().Be(new DateOnly(2024, 4, 15));

        typeof(PackAuthorisation).GetProperties()
            .Where(x => x.Name is nameof(PackAuthorisation.RegistrationId)
                or nameof(PackAuthorisation.PackagedProductId))
            .Should().OnlyContain(x => x.SetMethod!.IsPrivate);
    }

    [Fact]
    public void ACorrectionToNoDateIsRefused()
    {
        var correct = () => An().Correct(default);

        correct.Should().Throw<DomainException>()
            .WithMessage(PackAuthorisationErrors.AuthorisedOnRequired);
    }
}
