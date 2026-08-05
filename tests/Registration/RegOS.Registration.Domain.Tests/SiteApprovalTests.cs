using FluentAssertions;

using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.Registration.Domain.Aggregates.PackAuthorisations;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.Registration.Domain.Aggregates.SiteApprovals;
using RegOS.SharedKernel.Exceptions;
using RegOS.SharedKernel.Primitives;

namespace RegOS.Registration.Domain.Tests;

/// <summary>
/// EPIC-010c S002 — a licence approving one site, from a date.
/// </summary>
/// <remarks>
/// <b>The second occurrence of <em>licence + thing + date</em></b>, after
/// <see cref="PackAuthorisation"/>. Copied rather than abstracted: ADR-018 says
/// two is a pattern and three is when to evaluate — and EPIC-010b's retro is the
/// reminder that the evaluation may correctly return <em>no</em>.
/// </remarks>
public sealed class SiteApprovalTests
{
    private static readonly TenantId Tenant = new(Guid.NewGuid());
    private static readonly RegistrationId Licence = new(Guid.NewGuid());
    private static readonly OrganizationSiteId Cologne = OrganizationSiteId.New();

    private static SiteApproval An(
        OrganizationSiteId? site = null,
        DateOnly? on = null)
        => SiteApproval.Create(
            Tenant, Licence, site ?? Cologne, on ?? new DateOnly(2024, 3, 1));

    [Fact]
    public void AnApprovalIsALicenceASiteAndADate()
    {
        var approval = An();

        approval.RegistrationId.Should().Be(Licence);
        approval.OrganizationSiteId.Should().Be(Cologne);
        approval.ApprovedOn.Should().Be(new DateOnly(2024, 3, 1));
    }

    /// <summary>
    /// <b>The fact a foreign key could not carry, and the reason this type
    /// exists.</b> A licence granted in 2021 that added a secondary packaging
    /// site in 2024 by variation has two dates, and only one of them is the
    /// registration's.
    /// </summary>
    [Fact]
    public void ASiteJoinsALicenceLongAfterItWasGranted()
    {
        An(on: new DateOnly(2024, 6, 1))
            .ApprovedOn.Should().Be(new DateOnly(2024, 6, 1));
    }

    [Fact]
    public void AnApprovalNeedsTheDateItHappened()
    {
        var create = () => SiteApproval.Create(Tenant, Licence, Cologne, default);

        create.Should().Throw<DomainException>()
            .WithMessage(SiteApprovalErrors.ApprovedOnRequired);
    }

    [Fact]
    public void AnApprovalNeedsASite()
    {
        var create = () => SiteApproval.Create(
            Tenant, Licence, null!, new DateOnly(2024, 3, 1));

        create.Should().Throw<DomainException>()
            .WithMessage(SiteApprovalErrors.SiteRequired);
    }

    /// <summary>
    /// The pair is immutable: a different site or a different licence is a
    /// different approval, and editing one into another would leave no way to
    /// tell a correction from a replacement.
    /// </summary>
    [Fact]
    public void CorrectingTheDateLeavesThePairAlone()
    {
        var approval = An();

        approval.Correct(new DateOnly(2021, 6, 1));

        approval.ApprovedOn.Should().Be(new DateOnly(2021, 6, 1));
        approval.RegistrationId.Should().Be(Licence);
        approval.OrganizationSiteId.Should().Be(Cologne);
    }

    /// <summary>
    /// <b>A site has no market, and that is the difference from a pack.</b>
    /// <c>AuthorisePack</c> refuses a pack from another market because both the
    /// licence and the pack name a medicinal product and they must agree. One
    /// plant supplies licences in eight countries — there is nothing here to
    /// disagree, so this type holds no market and asserts none.
    /// </summary>
    [Fact]
    public void AnApprovalHoldsNoMarketToDisagreeAbout()
    {
        typeof(SiteApproval).GetProperties()
            .Select(x => x.Name)
            .Should().NotContain(name =>
                name.Contains("MedicinalProduct", StringComparison.Ordinal)
                || name.Contains("Country", StringComparison.Ordinal));
    }

    /// <summary>
    /// <b>It records that the licence names the site, and nothing about whether
    /// the site is fit to do the work.</b> That is a quality system's statement
    /// (EPIC-008), and keeping them apart is what lets S004 report a divergence
    /// between what a licence approves and what actually happens.
    /// </summary>
    [Fact]
    public void AnApprovalSaysNothingAboutWhatTheSiteDoes()
    {
        typeof(SiteApproval).GetProperties()
            .Select(x => x.Name)
            .Should().NotContain(name =>
                name.Contains("Operation", StringComparison.Ordinal)
                || name.Contains("Qualif", StringComparison.Ordinal));
    }

    /// <summary>
    /// <b>Two roots of the same kind, and deliberately not one.</b>
    /// </summary>
    /// <remarks>
    /// <b>There is no test here comparing this type's shape to
    /// <see cref="PackAuthorisation"/>'s, and the attempt is worth recording.</b>
    /// One was written and deleted: it compared the two aggregates' property
    /// types and could never have passed — one holds a <c>PackagedProductId</c>,
    /// the other an <c>OrganizationSiteId</c>. Bending it to pass would have
    /// meant asserting a <em>coincidence of shape</em> rather than a decision,
    /// and it would have broken the first time either type gained a field for a
    /// good reason.
    /// <para>
    /// The decision — <b>copy the pattern, do not abstract it at two</b>
    /// (ADR-018) — lives in both types' docstrings and in ADR-063 §4, which is
    /// where a reader looks for it. A third occurrence is the trigger to
    /// evaluate, and that is a judgement made once with all three in view; no
    /// test can make it.
    /// </para>
    /// </remarks>
    [Fact]
    public void TheApprovalAndThePackAuthorisationAreSeparateRoots()
    {
        typeof(SiteApproval).Should().NotBe(typeof(PackAuthorisation));

        // What they genuinely share, and all this asserts: the dated
        // relationship both exist to carry.
        typeof(SiteApproval).GetProperty(nameof(SiteApproval.ApprovedOn))!
            .PropertyType.Should().Be(
                typeof(PackAuthorisation)
                    .GetProperty(nameof(PackAuthorisation.AuthorisedOn))!
                    .PropertyType);
    }
}
