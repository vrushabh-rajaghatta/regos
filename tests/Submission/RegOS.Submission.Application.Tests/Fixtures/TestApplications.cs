using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Primitives;

using RegulatoryApplicationAggregate =
    RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;

namespace RegOS.Submission.Application.Tests.Fixtures;

/// <summary>
/// Guarantees the parent Product and Regulatory Application that submission
/// tests hang their fixtures from.
/// </summary>
/// <remarks>
/// <para>
/// These tests used to call <c>RegulatoryApplications.FirstAsync()</c> and
/// depend on whatever happened to be in the developer's database. That was
/// invisible until PROD-002 reset the product data and 30 tests failed at once.
/// Ensuring the row exists makes the suite independent of ambient state: it
/// passes against an empty database and against a populated one.
/// </para>
/// <para>
/// The fixture is identified by a fixed product code rather than "whatever came
/// back first", so that test classes running in parallel converge on one row
/// instead of each creating their own. The unique index on
/// (TenantId, Code) is what actually settles the race: the loser of a
/// concurrent insert catches the violation and re-reads the winner's row.
/// </para>
/// <para>
/// <b>Publishing classes pass their own <paramref name="fixtureCode"/>.</b> Once
/// a sequence number is scoped to an application (ADR-044), two test classes
/// sharing one application share a numbering space — and running in parallel
/// they contend on the unique index, so one of them gets a genuine 409 for
/// reasons that have nothing to do with what it was testing. A shared
/// application was harmless before numbering existed and is a test-isolation
/// defect after it.
/// </para>
/// </remarks>
internal static class TestApplications
{
    private const string DefaultFixtureCode = "TEST-FIXTURE";

    public static async Task<(RegulatoryApplicationId AppId, GlobalProductId GlobalProductId)>
        EnsureAsync(RegOSDbContext ctx, string fixtureCode = DefaultFixtureCode)
    {
        var existing = await FindAsync(ctx, fixtureCode);

        if (existing is not null)
            return existing.Value;

        var organizationId = await ctx.Organizations
            .AsNoTracking().Select(x => x.Id).FirstAsync();
        var countryId = await ctx.Countries
            .AsNoTracking().Select(x => x.Id).FirstAsync();
        var authorityId = await ctx.Authorities
            .AsNoTracking().Select(x => x.Id).FirstAsync();

        // The product's owner is a tenant; the application's applicant is an
        // organization (ADR-030 split them). The tenant is pinned to the one
        // every Submission test's DbContext is scoped to — under the global
        // query filter (ADR-031) a fixture created for any other tenant would
        // be invisible to the tests that need it.
        var tenantId = TestTenant.Id;

        var product = GlobalProduct.Register(
            tenantId, fixtureCode, "Submission Test Product", ProductType.Drug);

        var application = RegulatoryApplicationAggregate.Create(
            tenantId,
            product.Id, countryId, authorityId, organizationId,
            $"Submission Test Application ({fixtureCode})");

        ctx.Products.Add(product);
        ctx.RegulatoryApplications.Add(application);

        try
        {
            await ctx.SaveChangesAsync();

            return (application.Id, product.Id);
        }
        catch (DbUpdateException)
        {
            // Another test class won the race. Drop our unsaved entities and
            // use theirs.
            ctx.ChangeTracker.Clear();

            return await FindAsync(ctx, fixtureCode)
                ?? throw new InvalidOperationException(
                    "The shared submission fixture could not be created or found.");
        }
    }

    private static async Task<(RegulatoryApplicationId, GlobalProductId)?> FindAsync(
        RegOSDbContext ctx, string fixtureCode)
    {
        var code = ProductCode.Create(fixtureCode);

        var globalProductId = await ctx.Products
            .AsNoTracking()
            .Where(x => x.Code == code)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        if (globalProductId is null)
            return null;

        var applicationId = await ctx.RegulatoryApplications
            .AsNoTracking()
            .Where(x => x.GlobalProductId == globalProductId)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        return applicationId == default
            ? null
            : (applicationId, globalProductId);
    }
}
