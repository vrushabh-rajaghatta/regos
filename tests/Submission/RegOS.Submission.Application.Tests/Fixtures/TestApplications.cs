using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;
using RegOS.SharedKernel.Primitives;

using ProductAggregate = RegOS.Product.Domain.Product.Product;
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
/// </remarks>
internal static class TestApplications
{
    private const string FixtureCode = "TEST-FIXTURE";

    public static async Task<(RegulatoryApplicationId AppId, ProductId ProductId)>
        EnsureAsync(RegOSDbContext ctx)
    {
        var existing = await FindAsync(ctx);

        if (existing is not null)
            return existing.Value;

        var organizationId = await ctx.Organizations
            .AsNoTracking().Select(x => x.Id).FirstAsync();
        var countryId = await ctx.Countries
            .AsNoTracking().Select(x => x.Id).FirstAsync();
        var authorityId = await ctx.Authorities
            .AsNoTracking().Select(x => x.Id).FirstAsync();

        // The product's owner is a tenant; the application's applicant is an
        // organization (ADR-030 split them). The seeded tenants share their
        // guids with the seeded organizations, so aligning the two keeps the
        // fixture equivalent to a customer filing for itself.
        var tenantId = new TenantId(organizationId.Value);

        var product = ProductAggregate.Register(
            tenantId, FixtureCode, "Submission Test Product", ProductType.Drug);

        var application = RegulatoryApplicationAggregate.Create(
            product.Id, countryId, authorityId, organizationId,
            "Submission Test Application");

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

            return await FindAsync(ctx)
                ?? throw new InvalidOperationException(
                    "The shared submission fixture could not be created or found.");
        }
    }

    private static async Task<(RegulatoryApplicationId, ProductId)?> FindAsync(
        RegOSDbContext ctx)
    {
        var code = ProductCode.Create(FixtureCode);

        var productId = await ctx.Products
            .AsNoTracking()
            .Where(x => x.Code == code)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        if (productId is null)
            return null;

        var applicationId = await ctx.RegulatoryApplications
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        return applicationId == default
            ? null
            : (applicationId, productId);
    }
}
