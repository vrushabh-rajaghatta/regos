using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.Regulatory.Authority;
using RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication;

using RegulatoryApplicationAggregate =
    RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;

namespace RegOS.Submission.Application.Tests.Fixtures;

/// <summary>
/// A parent application pinned to the <b>FDA</b>, for tests that need blueprint
/// resolution to succeed.
/// </summary>
/// <remarks>
/// Distinct from <see cref="TestApplications"/>, which takes whichever authority
/// comes back first: creating a submission requires the submission type to
/// belong to the application's authority, so any test about FDA blueprints needs
/// an FDA application specifically.
/// <para>
/// Find-or-create against a fixed product code, for the reason TestApplications
/// documents: parallel test classes must converge on one row, and the unique
/// index on (TenantId, Code) settles the race.
/// </para>
/// </remarks>
internal static class TestFdaApplication
{
    private const string FixtureCode = "TEST-BLUEPRINT-FDA";

    public static readonly AuthorityId Fda =
        new(Guid.Parse("20000000-0000-0000-0000-000000000001"));

    public static async Task<(RegulatoryApplicationId AppId, GlobalProductId GlobalProductId)>
        EnsureAsync(RegOSDbContext ctx)
    {
        var existing = await FindAsync(ctx);

        if (existing is not null)
            return existing.Value;

        var countryId = await ctx.Countries
            .AsNoTracking().Select(x => x.Id).FirstAsync();
        var organizationId = await ctx.Organizations
            .AsNoTracking().Select(x => x.Id).FirstAsync();

        var product = GlobalProduct.Register(
            TestTenant.Id, FixtureCode, "Blueprint Validation Product", ProductType.Drug);

        var application = RegulatoryApplicationAggregate.Create(
            TestTenant.Id,
            product.Id,
            countryId,
            Fda,
            organizationId,
            "Blueprint Validation Application");

        ctx.Products.Add(product);
        ctx.RegulatoryApplications.Add(application);

        try
        {
            await ctx.SaveChangesAsync();

            return (application.Id, product.Id);
        }
        catch (DbUpdateException)
        {
            // Another test class won the race — use its row.
            ctx.ChangeTracker.Clear();

            return await FindAsync(ctx)
                ?? throw new InvalidOperationException(
                    "The FDA blueprint fixture could not be created or found.");
        }
    }

    private static async Task<(RegulatoryApplicationId, GlobalProductId)?> FindAsync(
        RegOSDbContext ctx)
    {
        var code = ProductCode.Create(FixtureCode);

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

        return applicationId == default ? null : (applicationId, globalProductId);
    }
}
