using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Application.Commands.CreateContact;
using RegOS.Organization.Application.Commands.CreateOrganizationSite;
using RegOS.Organization.Application.Queries.Contacts.ContactDirectory;
using RegOS.Organization.Application.Queries.Contacts.GetContact;
using RegOS.Organization.Application.Queries.Contacts.ListOrganizationContacts;
using RegOS.Organization.Application.Services;
using RegOS.Organization.Application.Tests.Fixtures;
using RegOS.Organization.Domain.Aggregates.Contact;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.Organization.Infrastructure.Persistence;
using RegOS.Organization.Infrastructure.Services;
using RegOS.Persistence;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Organization;
using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Exceptions;

using ContactAggregate = RegOS.Organization.Domain.Aggregates.Contact.Contact;
using OrganizationAggregate = RegOS.Organization.Domain.Aggregates.Organization.Organization;

namespace RegOS.Organization.Application.Tests;

/// <summary>
/// Integration tests — contacts against the real seeded reference data in the
/// dev Postgres.
/// </summary>
[Collection(OrganizationDatabase.Collection)]
public sealed class ContactTests : IAsyncLifetime
{
    private readonly OrganizationDatabase _database;

    public ContactTests(OrganizationDatabase database)
    {
        _database = database;
    }


    private static readonly CountryId India =
        new(Guid.Parse("10000000-0000-0000-0000-000000000004"));

    /// <summary>Qualified Person — seeded with a null tenant.</summary>
    private static readonly ContactRoleId QualifiedPerson =
        new(Guid.Parse("81000000-0000-0000-0000-000000000001"));

    private static readonly ContactRoleId RegulatoryContact =
        new(Guid.Parse("81000000-0000-0000-0000-000000000003"));

    private static readonly DateOnly Joined = new(2021, 9, 1);

    private readonly List<Guid> _contactIds = [];
    private readonly List<Guid> _siteIds = [];
    private readonly List<Guid> _organizationIds = [];
    private readonly List<Guid> _roleIds = [];

    private RegOSDbContext New(ITenantContext? tenant = null) =>
        new(
            _database.Options,
            tenant ?? TestTenants.ActingContext);

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var ctx = New();

        foreach (var id in _contactIds)
        {
            var contact = await ctx.Contacts
                .Include(x => x.Roles)
                .Include(x => x.Emails)
                .Include(x => x.Phones)
                .FirstOrDefaultAsync(x => x.Id == new ContactId(id));

            if (contact is not null)
                ctx.Contacts.Remove(contact);
        }

        await ctx.SaveChangesAsync();

        foreach (var id in _siteIds)
        {
            var site = await ctx.OrganizationSites
                .Include(x => x.Identifiers)
                .FirstOrDefaultAsync(x => x.Id == new OrganizationSiteId(id));

            if (site is not null)
                ctx.OrganizationSites.Remove(site);
        }

        foreach (var id in _roleIds)
        {
            var role = await ctx.ContactRoles
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == new ContactRoleId(id));

            if (role is not null)
                ctx.ContactRoles.Remove(role);
        }

        await ctx.SaveChangesAsync();

        foreach (var id in _organizationIds)
        {
            var organization = await ctx.Organizations
                .FirstOrDefaultAsync(x => x.Id == new OrganizationId(id));

            if (organization is not null)
                ctx.Organizations.Remove(organization);
        }

        await ctx.SaveChangesAsync();
    }

    // --- Creating ------------------------------------------------------------

    [Fact]
    public async Task AContactIsPersistedWithRolesEmailsAndPhones()
    {
        await using var ctx = New();
        var organizationId = await OrganizationAsync(ctx);

        var id = await CreateAsync(
            ctx,
            organizationId,
            roleIds: [QualifiedPerson, RegulatoryContact],
            emails: ["priya.raman@example.com", "qp@example.com"],
            phones: [new ContactPhoneInput(
                "+91 40 1234 5678", ContactPhoneKind.Business)]);

        await using var check = New();
        var contact = await check.Contacts
            .AsNoTracking()
            .Include(x => x.Roles)
            .Include(x => x.Emails)
            .Include(x => x.Phones)
            .FirstAsync(x => x.Id == id);

        contact.TenantId.Should().Be(TestTenants.Acting);
        contact.Status.Should().Be(OrganizationStatus.Active);
        contact.Roles.Should().HaveCount(2);
        contact.Emails.Should().HaveCount(2);

        // Stored as its name (see ContactPhoneConfiguration), so this is also
        // the round-trip through the string conversion.
        contact.Phones.Single().Kind.Should().Be(ContactPhoneKind.Business);
        contact.Phones.Should().ContainSingle();
        contact.OrganizationSiteId.Should().BeNull();
    }

    [Fact]
    public async Task AContactCanBeSitedAtOneOfItsOrganizationsSites()
    {
        await using var ctx = New();
        var organizationId = await OrganizationAsync(ctx);
        var siteId = await SiteAsync(ctx, organizationId);

        var id = await CreateAsync(ctx, organizationId, siteId: siteId);

        await using var check = New();
        var detail = await new GetContactHandler(check).HandleAsync(new GetContactQuery(id), default);

        detail!.SiteId.Should().Be(siteId.Value);
        detail.SiteName.Should().NotBeNullOrWhiteSpace();
    }

    /// <summary>
    /// A person cannot work at a site their employer does not operate.
    /// </summary>
    [Fact]
    public async Task AContactCannotBeSitedAtAnotherOrganizationsSite()
    {
        await using var ctx = New();
        var employer = await OrganizationAsync(ctx);
        var elsewhere = await OrganizationAsync(ctx);
        var foreignSite = await SiteAsync(ctx, elsewhere);

        var create = async () =>
            await CreateAsync(ctx, employer, siteId: foreignSite);

        await create.Should().ThrowAsync<DomainException>()
            .WithMessage(ContactRuleErrors.SiteNotForOrganization);
    }

    [Fact]
    public async Task AnUnknownOrganizationIsNotFound()
    {
        await using var ctx = New();

        var create = async () => await CreateAsync(ctx, OrganizationId.New());

        await create.Should().ThrowAsync<NotFoundException>()
            .WithMessage(ContactRuleErrors.OrganizationDoesNotExist);
    }

    [Fact]
    public async Task AnUnknownRoleIsRejected()
    {
        await using var ctx = New();
        var organizationId = await OrganizationAsync(ctx);

        var create = async () => await CreateAsync(
            ctx, organizationId, roleIds: [ContactRoleId.New()]);

        await create.Should().ThrowAsync<DomainException>()
            .WithMessage(ContactRuleErrors.RoleDoesNotExist);
    }

    [Fact]
    public async Task TheSameRoleTwiceIsRefused()
    {
        await using var ctx = New();
        var organizationId = await OrganizationAsync(ctx);

        var create = async () => await CreateAsync(
            ctx, organizationId, roleIds: [QualifiedPerson, QualifiedPerson]);

        await create.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage(ContactErrors.RoleAlreadyHeld);
    }

    // --- Roles are shared plus extensible ------------------------------------

    /// <summary>
    /// The distinction from <c>IdentifierScheme</c>: a scheme describes the
    /// outside world, a role describes how a company organises people. So the
    /// platform ships a baseline every tenant sees, and a tenant's own additions
    /// stay private to them.
    /// </summary>
    [Fact]
    public async Task PlatformRolesAreSharedAndTenantRolesAreNot()
    {
        await using var ctx = New();

        var seeded = await ctx.ContactRoles
            .AsNoTracking()
            .Select(x => x.Code)
            .ToListAsync();

        seeded.Should().Contain(["QP", "AR", "REG", "PV"]);

        // A role this tenant coins for itself.
        var ourOwn = ContactRole.Create(
            ContactRoleId.New(),
            $"APAC-LEAD-{Guid.NewGuid():N}"[..20],
            "APAC Regulatory Lead",
            tenantId: TestTenants.Acting);

        ctx.ContactRoles.Add(ourOwn);
        await ctx.SaveChangesAsync();
        _roleIds.Add(ourOwn.Id.Value);

        await using var ours = New();
        (await ours.ContactRoles.AsNoTracking()
            .AnyAsync(x => x.Id == ourOwn.Id))
            .Should().BeTrue();

        // The other tenant sees the platform's roles but not ours.
        await using var other = New(TestTenants.OtherContext);

        (await other.ContactRoles.AsNoTracking()
            .AnyAsync(x => x.Id == ourOwn.Id))
            .Should().BeFalse("a tenant's own vocabulary is its own");

        (await other.ContactRoles.AsNoTracking()
            .AnyAsync(x => x.Code == "QP"))
            .Should().BeTrue("the platform's baseline is shared");
    }

    // --- Tenant isolation ----------------------------------------------------

    [Fact]
    public async Task AnotherTenantSeesNoneOfTheseContacts()
    {
        await using var ctx = New();
        var organizationId = await OrganizationAsync(ctx);
        var id = await CreateAsync(
            ctx, organizationId, roleIds: [QualifiedPerson]);

        await using var intruder = New(TestTenants.OtherContext);

        (await intruder.Contacts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id))
            .Should().BeNull();

        (await new ContactDirectoryHandler(intruder)
            .HandleAsync(new ContactDirectoryQuery(QualifiedPerson), default))
            .Should().NotContain(x => x.ContactId == id.Value);

        (await new GetContactHandler(intruder).HandleAsync(new GetContactQuery(id), default))
            .Should().BeNull();
    }

    // --- The directory -------------------------------------------------------

    /// <summary>
    /// "Who is the QP?" — the query that makes Contact a root, across the whole
    /// registry rather than within one company.
    /// </summary>
    [Fact]
    public async Task TheDirectoryAnswersByRole()
    {
        await using var ctx = New();
        var organizationId = await OrganizationAsync(ctx);

        var qp = await CreateAsync(
            ctx, organizationId, roleIds: [QualifiedPerson]);
        var regulatory = await CreateAsync(
            ctx, organizationId, roleIds: [RegulatoryContact]);

        await using var check = New();
        var handler = new ContactDirectoryHandler(check);

        var qps = await handler.HandleAsync(new ContactDirectoryQuery(QualifiedPerson), default);
        qps.Should().Contain(x => x.ContactId == qp.Value);
        qps.Should().NotContain(x => x.ContactId == regulatory.Value);

        // The directory spans the registry, so it names the employer.
        qps.Single(x => x.ContactId == qp.Value)
            .OrganizationName.Should().NotBeNullOrWhiteSpace();

        // The role reads as a name, not an id.
        qps.Single(x => x.ContactId == qp.Value)
            .Roles.Should().ContainSingle()
            .Which.Name.Should().Be("Qualified Person");

        var everyone = await handler.HandleAsync(new ContactDirectoryQuery(), default);
        everyone.Should().Contain(x => x.ContactId == qp.Value);
        everyone.Should().Contain(x => x.ContactId == regulatory.Value);
    }

    /// <summary>
    /// Nothing is hidden: the person named on a 2019 licence is still that
    /// person, whether or not they still work there.
    /// </summary>
    [Fact]
    public async Task TheDirectoryKeepsFormerContactsAndMarksThem()
    {
        await using var ctx = New();
        var organizationId = await OrganizationAsync(ctx);
        var id = await CreateAsync(
            ctx, organizationId, roleIds: [QualifiedPerson]);

        await using var act = New();
        var repository = new ContactRepository(act);
        var contact = await repository.GetByIdAsync(id, default);
        contact!.Deactivate(new DateOnly(2026, 2, 28));
        await repository.UpdateAsync(contact, default);

        await using var check = New();
        var row = (await new ContactDirectoryHandler(check)
            .HandleAsync(new ContactDirectoryQuery(QualifiedPerson), default))
            .Single(x => x.ContactId == id.Value);

        row.Status.Should().Be(nameof(OrganizationStatus.Inactive));
        row.StatusDate.Should().Be(new DateOnly(2026, 2, 28));
    }

    [Fact]
    public async Task AnOrganizationListsItsOwnPeople()
    {
        await using var ctx = New();
        var organizationId = await OrganizationAsync(ctx);
        await CreateAsync(
            ctx, organizationId,
            roleIds: [QualifiedPerson],
            emails: ["qp@example.com"]);

        await using var check = New();
        var contacts = await new ListOrganizationContactsHandler(check)
            .HandleAsync(new ListOrganizationContactsQuery(organizationId), default);

        var row = contacts!.Should().ContainSingle().Subject;

        row.Emails.Should().ContainSingle().Which.Should().Be("qp@example.com");
        row.Roles.Should().ContainSingle().Which.Code.Should().Be("QP");
    }

    [Fact]
    public async Task AnUnknownOrganizationHasNoContactList()
    {
        await using var ctx = New();

        (await new ListOrganizationContactsHandler(ctx)
            .HandleAsync(new ListOrganizationContactsQuery(OrganizationId.New()), default))
            .Should().BeNull();
    }

    // --- helpers -------------------------------------------------------------

    private async Task<ContactId> CreateAsync(
        RegOSDbContext ctx,
        OrganizationId organizationId,
        OrganizationSiteId? siteId = null,
        IReadOnlyList<ContactRoleId>? roleIds = null,
        IReadOnlyList<string>? emails = null,
        IReadOnlyList<ContactPhoneInput>? phones = null)
    {
        var handler = new CreateContactHandler(
            new ContactCreationPolicy(ctx),
            new ContactRepository(ctx),
            TestTenants.ActingContext);

        var id = await handler.HandleAsync(
            new CreateContactCommand(
                organizationId,
                "Priya",
                $"Raman{Guid.NewGuid():N}"[..12],
                Joined,
                siteId,
                RoleIds: roleIds,
                Emails: emails,
                Phones: phones),
            default);

        _contactIds.Add(id.Value);

        return id;
    }

    private async Task<OrganizationSiteId> SiteAsync(
        RegOSDbContext ctx,
        OrganizationId organizationId)
    {
        var handler = new CreateOrganizationSiteHandler(
            new OrganizationSiteCreationPolicy(ctx),
            new OrganizationSiteRepository(ctx),
            TestTenants.ActingContext);

        var id = await handler.HandleAsync(
            new CreateOrganizationSiteCommand(
                organizationId,
                $"Site {Guid.NewGuid():N}",
                OrganizationSiteType.Manufacturing,
                India,
                new DateOnly(2014, 5, 1)),
            default);

        _siteIds.Add(id.Value);

        return id;
    }

    private async Task<OrganizationId> OrganizationAsync(RegOSDbContext ctx)
    {
        var organization = OrganizationAggregate.Create(
            TestTenants.Acting,
            $"Contact Partner {Guid.NewGuid():N}",
            OrganizationType.Manufacturer);

        ctx.Organizations.Add(organization);
        await ctx.SaveChangesAsync();
        _organizationIds.Add(organization.Id.Value);

        return organization.Id;
    }
}
