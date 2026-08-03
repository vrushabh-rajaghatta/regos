using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Domain.Aggregates.Contact;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Persistence;
using RegOS.ReferenceData.Domain.Organization;
using RegOS.SharedKernel.Exceptions;
using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.Submission.Application.Queries.GetApplicationContacts;
using RegOS.Submission.Application.Tests.Fixtures;
using RegOS.Submission.Domain.Submission;
using RegOS.Submission.Infrastructure.Repositories;
using RegOS.Submission.Infrastructure.Services;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;

namespace RegOS.Submission.Application.Tests;

/// <summary>
/// **Who currently speaks for an application — derived, never stored**
/// (ADR-048).
/// </summary>
/// <remarks>
/// This is the test that makes the absence of <c>ApplicationContact</c> a
/// design rather than an omission. Under the cumulative model the latest
/// published sequence <em>is</em> the current regulatory state, so these
/// assertions are what a stored copy would have to agree with — and could only
/// disagree with by being stale.
/// <para>
/// Its own fixture application, for the reason <see cref="TestApplications"/>
/// gives: this class publishes, and a shared application is a shared numbering
/// space.
/// </para>
/// </remarks>
public sealed class ApplicationContactsTests : IAsyncLifetime
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=regos;Username=admin;Password=password123";

    private const string Fixture = "TEST-APPLICATION-CONTACTS";

    private static readonly ApplicationTypeId SeededApplicationType =
        new(Guid.Parse("40000000-0000-0000-0000-000000000001"));

    private static readonly OrganizationId DemoManufacturer =
        OrganizationId.From(Guid.Parse("30000000-0000-0000-0000-000000000001"));

    private static readonly ContactRoleId QualifiedPerson =
        new(Guid.Parse("81000000-0000-0000-0000-000000000001"));
    private static readonly ContactRoleId RegulatoryContact =
        new(Guid.Parse("81000000-0000-0000-0000-000000000003"));

    private readonly List<Guid> _submissionIds = [];
    private readonly List<Guid> _contactIds = [];

    private static RegOSDbContext New() =>
        new(new DbContextOptionsBuilder<RegOSDbContext>()
                .UseNpgsql(ConnectionString).Options,
            TestTenant.Context);

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await using var ctx = New();

        // Submissions first: SubmissionRoles cascade from them, and the foreign
        // key to Contacts is Restrict — so deleting a contact while a naming
        // still points at it would fail (ADR-048).
        if (_submissionIds.Count > 0)
        {
            await ctx.Database.ExecuteSqlRawAsync(
                "DELETE FROM \"Submissions\" WHERE \"Id\" = ANY({0})",
                new object[] { _submissionIds.ToArray() });
        }

        if (_contactIds.Count > 0)
        {
            await ctx.Database.ExecuteSqlRawAsync(
                "DELETE FROM \"Contacts\" WHERE \"Id\" = ANY({0})",
                new object[] { _contactIds.ToArray() });
        }
    }

    /// <summary>
    /// Before the first filing there is nobody named on a filing. An absence of
    /// a filing, not missing data — and deliberately not an error.
    /// </summary>
    [Fact]
    public async Task AnApplicationThatHasPublishedNothing_HasNoContacts()
    {
        var applicationId = await ApplicationAsync();
        await DraftAsync(applicationId, await ContactAsync("Ana", "Ruiz"));

        var contacts = await QueryAsync(applicationId);

        contacts.AsOfSequenceNumber.Should().BeNull();
        contacts.Contacts.Should().BeEmpty();
    }

    [Fact]
    public async Task TheFirstPublishedSequence_SuppliesTheContacts()
    {
        var applicationId = await ApplicationAsync();
        var qp = await ContactAsync("Ana", "Ruiz");

        var submissionId = await DraftAsync(applicationId, qp);
        await PublishAsync(submissionId);

        var contacts = await QueryAsync(applicationId);

        contacts.AsOfSequenceNumber.Should().Be(0);
        contacts.Contacts.Should().ContainSingle()
            .Which.ContactName.Should().Be("Ana Ruiz");
    }

    /// <summary>
    /// <b>The heart of it.</b> The latest sequence <em>is</em> the state — not
    /// the union of every sequence's people. Sequence 0001 naming somebody else
    /// replaces the answer rather than adding to it, which is exactly what a
    /// stored application-level copy would have had to be kept in step with.
    /// </summary>
    [Fact]
    public async Task ALaterSequence_ReplacesTheAnswerRatherThanAddingToIt()
    {
        var applicationId = await ApplicationAsync();
        var first = await ContactAsync("Ana", "Ruiz");
        var second = await ContactAsync("Bo", "Nilsen");

        await PublishAsync(await DraftAsync(applicationId, first));
        await PublishAsync(await DraftAsync(applicationId, second));

        var contacts = await QueryAsync(applicationId);

        contacts.AsOfSequenceNumber.Should().Be(1);
        contacts.Contacts.Should().ContainSingle()
            .Which.ContactName.Should().Be("Bo Nilsen");
    }

    /// <summary>
    /// A draft is not a filing. Naming somebody on one changes nothing about
    /// who currently speaks for the application.
    /// </summary>
    [Fact]
    public async Task ADraftsPeople_AreNotTheApplicationsContacts()
    {
        var applicationId = await ApplicationAsync();
        var filed = await ContactAsync("Ana", "Ruiz");
        var proposed = await ContactAsync("Cy", "Okafor");

        await PublishAsync(await DraftAsync(applicationId, filed));
        await DraftAsync(applicationId, proposed);

        var contacts = await QueryAsync(applicationId);

        contacts.AsOfSequenceNumber.Should().Be(0);
        contacts.Contacts.Should().ContainSingle()
            .Which.ContactName.Should().Be("Ana Ruiz");
    }

    [Fact]
    public async Task EveryNamingOnTheLatestSequence_IsReturned()
    {
        var applicationId = await ApplicationAsync();
        var person = await ContactAsync("Ana", "Ruiz");

        var submissionId = await DraftAsync(applicationId, person);

        await using (var ctx = New())
        {
            var submission = await new SubmissionRepository(ctx)
                .GetByIdAsync(SubmissionId.From(submissionId), default);

            submission!.AssignRole(person, RegulatoryContact);

            await ctx.SaveChangesAsync();
        }

        await PublishAsync(submissionId);

        var contacts = await QueryAsync(applicationId);

        contacts.Contacts.Should().HaveCount(2);
        contacts.Contacts.Select(x => x.RoleName).Should()
            .BeEquivalentTo(["Qualified Person", "Regulatory Contact"]);
    }

    // --- what only a round trip can catch ------------------------------------

    /// <summary>
    /// <b>Removing a naming needs the collection to have been loaded.</b> The
    /// aggregate searches <c>_roles</c>, so a repository that does not include
    /// it turns every removal into a silent not-found — which is exactly what
    /// happened, and no unit test could see it because an in-memory aggregate
    /// always has its collection populated.
    /// </summary>
    [Fact]
    public async Task ANaming_CanBeRemovedAfterALoad()
    {
        var applicationId = await ApplicationAsync();
        var person = await ContactAsync("Dee", "Marek");
        var submissionId = await DraftAsync(applicationId, person);

        await using (var ctx = New())
        {
            var repository = new SubmissionRepository(ctx);
            var submission = await repository.GetByIdAsync(
                SubmissionId.From(submissionId), default);

            submission!.Roles.Should().ContainSingle(
                "the repository must load the collection the aggregate reasons over");

            submission.RemoveRole(submission.Roles.Single().Id);

            await repository.UpdateAsync(submission, default);
        }

        await using var check = New();
        var reloaded = await new SubmissionRepository(check)
            .GetByIdAsync(SubmissionId.From(submissionId), default);

        reloaded!.Roles.Should().BeEmpty();
    }

    /// <summary>
    /// The duplicate is refused by the <em>domain</em>, not by the unique index.
    /// With an unloaded collection the aggregate's check is vacuously true and
    /// Postgres fails the insert instead — a 500 where a business rule belongs.
    /// </summary>
    [Fact]
    public async Task NamingTheSamePersonTwice_IsRefusedByTheDomain()
    {
        var applicationId = await ApplicationAsync();
        var person = await ContactAsync("Eli", "Sandoval");
        var submissionId = await DraftAsync(applicationId, person);

        await using var ctx = New();
        var submission = await new SubmissionRepository(ctx)
            .GetByIdAsync(SubmissionId.From(submissionId), default);

        var act = () => submission!.AssignRole(person, QualifiedPerson);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(SubmissionErrors.ContactAlreadyNamedInThatRole);
    }

    // --- helpers -------------------------------------------------------------

    private static async Task<ApplicationContacts> QueryAsync(
        RegulatoryApplicationId applicationId)
    {
        await using var ctx = New();

        return await new GetApplicationContactsHandler(ctx)
            .HandleAsync(new GetApplicationContactsQuery(applicationId), default);
    }

    private static async Task<RegulatoryApplicationId> ApplicationAsync()
    {
        await using var ctx = New();

        return (await TestApplications.EnsureAsync(ctx, Fixture)).AppId;
    }

    private async Task<ContactId> ContactAsync(string first, string last)
    {
        await using var ctx = New();

        var contact = Contact.Create(
            TestTenant.Id,
            DemoManufacturer,
            first,
            $"{last}",
            DateOnly.FromDateTime(DateTime.UtcNow));

        ctx.Contacts.Add(contact);
        await ctx.SaveChangesAsync();

        _contactIds.Add(contact.Id.Value);

        return contact.Id;
    }

    private async Task<Guid> DraftAsync(
        RegulatoryApplicationId applicationId,
        ContactId contactId)
    {
        await using var ctx = New();

        var submission = SubmissionAggregate.Create(
            TestTenant.Id,
            applicationId,
            "Contacts Sub " + Guid.NewGuid(),
            SubmissionFormat.Ectd,
            TestSubmissionClassification.Opens());

        submission.AssignRole(contactId, QualifiedPerson);

        ctx.Submissions.Add(submission);
        await ctx.SaveChangesAsync();

        _submissionIds.Add(submission.Id.Value);

        return submission.Id.Value;
    }

    private static async Task PublishAsync(Guid submissionId)
    {
        await using var ctx = New();

        var repository = new SubmissionRepository(ctx);
        var submission = await repository.GetByIdAsync(
            SubmissionId.From(submissionId), default);

        var baseline = await new SubmissionPublicationBaseline(ctx)
            .GetAsync(submission!.ApplicationId, default);

        submission.Publish(
            baseline.NextSequenceNumber,
            baseline.PreviousPublishedSequenceNumber,
            baseline.Placements,
            DateTimeOffset.UtcNow);

        await repository.UpdateAsync(submission, default);
    }
}
