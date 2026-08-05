using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using Npgsql;

namespace RegOS.ProductDocument.Persistence.Tests;

/// <summary>
/// <b>EPIC-023's capstone — the assertion the epic exists to make true.</b>
/// </summary>
/// <remarks>
/// <para>
/// The finding was not that the suite was red. It was that
/// <b>green meant "nothing collided", not "the schema is current"</b>: a stale
/// schema only turns a test red when a migration happens to touch a read path
/// some test already exercises. EPIC-022 S004 ran one migration behind and
/// passed; two stories later the database was five behind and 18 of 19 suites
/// failed at once. Nothing in between said a word.
/// </para>
/// <para>
/// So this asserts the thing that was previously assumed. It lives in this
/// assembly because this is the one S001 converted first, and one placement is
/// enough — the guarantee itself is structural, enforced for all seven by
/// <c>RegOSTestDatabase</c>. What these tests add is a <em>readable</em>
/// statement of it, which a person can run and believe.
/// </para>
/// </remarks>
[Collection(ProductDocumentDatabase.Collection)]
public sealed class SchemaCurrencyTests
{
    private readonly ProductDocumentDatabase _database;

    public SchemaCurrencyTests(ProductDocumentDatabase database)
    {
        _database = database;
    }

    /// <summary>
    /// <b>The one that would have caught it.</b> Run against the developer's
    /// database on 2026-08-04 this fails, naming the five missing migrations;
    /// run against the same database on 2026-08-05 it fails naming one, on a
    /// day the whole suite was green.
    /// </summary>
    [Fact]
    public async Task The_schema_this_suite_runs_against_holds_every_migration()
    {
        await using var context = _database.NewContext();

        var pending = await context.Database.GetPendingMigrationsAsync();

        pending.Should().BeEmpty(
            "the suite provisions its own database from the migration chain, so "
            + "there is no window in which a schema can be behind it (ADR-064)");

        _database.AppliedMigrations.Should().BeEquivalentTo(
            context.Database.GetMigrations(),
            "every migration in source control should be recorded as applied — "
            + "a shorter list means the schema came from somewhere other than "
            + "the chain");
    }

    /// <summary>
    /// <b>And it is nobody's working database.</b> The assertion above would
    /// also pass against a hand-maintained database that somebody had just
    /// migrated — which is the state the project was in for a year, and the one
    /// that kept quietly reverting.
    /// </summary>
    [Fact]
    public void This_is_not_a_database_anyone_maintains()
    {
        _database.Name.Should().StartWith("regos_test_",
            "a provisioned database is created for this run and dropped after "
            + "it; if the suite is pointed at a durable one, every guarantee "
            + "here is back to depending on somebody having remembered");

        new NpgsqlConnectionStringBuilder(_database.ConnectionString)
            .Database.Should().NotBe("regos");
    }

    /// <summary>
    /// <b>The seed is the second thing worth proving</b> (ADR-064 §4). The
    /// application tier's tests lean on seeded reference data being present, so
    /// a database that migrates and does not seed would fail them for a reason
    /// that has nothing to do with the code under test.
    /// </summary>
    [Fact]
    public async Task The_real_initializer_chain_ran_against_it()
    {
        await using var context = _database.NewContext();

        // Countries and document types are seeded by the first initializer and
        // by one registered eleven later; both present means the chain ran to
        // completion rather than as far as its first failure.
        (await context.Countries.CountAsync()).Should().BeGreaterThan(0);

        // IgnoreQueryFilters on the tenant-owned ones, because this context has
        // no tenant — which is not a workaround but the seeding view itself
        // (ADR-031). The initializers are written against exactly this, and
        // several call IgnoreQueryFilters for the same reason: a filtered read
        // reports an empty table at boot and would re-insert every time.
        (await context.DocumentTypes.IgnoreQueryFilters().CountAsync())
            .Should().BeGreaterThan(0);

        // Seeded last of all, and only reachable if IdentifierSchemeDataInitializer
        // ran before it — the ordering EPIC-010c found by booting an empty
        // database and getting a foreign-key violation.
        (await context.OrganizationSites.IgnoreQueryFilters().CountAsync())
            .Should().BeGreaterThan(0);
    }
}
