using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.ApplicationType;
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
/// <b>The application type is the fixture's defining property</b> (EPIC-007a
/// S001). A blueprint binds to an application type, and the type belongs to the
/// application rather than to each sequence — so "an IND submission" and "a
/// 510(k) submission" are no longer two submissions under one application, they
/// are submissions under two applications. Each type therefore needs its own
/// product, because the unique index on
/// (GlobalProductId, CountryId, AuthorityId) permits only one application per
/// product for a given authority.
/// <para>
/// Find-or-create against a fixed product code, for the reason TestApplications
/// documents: parallel test classes must converge on one row, and the unique
/// index on (TenantId, Code) settles the race.
/// </para>
/// </remarks>
internal static class TestFdaApplication
{
    public static readonly AuthorityId Fda =
        new(Guid.Parse("20000000-0000-0000-0000-000000000001"));

    private static readonly ApplicationTypeId FdaInd =
        new(Guid.Parse("40000000-0000-0000-0000-000000000008"));

    private static readonly ApplicationTypeId Fda510k =
        new(Guid.Parse("40000000-0000-0000-0000-000000000001"));

    /// <summary>An FDA <b>IND</b> application — the CTD blueprint targets it.</summary>
    public static Task<(RegulatoryApplicationId AppId, GlobalProductId GlobalProductId)>
        EnsureAsync(RegOSDbContext ctx)
        => EnsureAsync(ctx, FdaInd, "TEST-BLUEPRINT-FDA");

    /// <summary>
    /// An FDA <b>510(k)</b> application — a device type under the same
    /// authority, which no blueprint targets. Submissions under it are
    /// deliberately unbound.
    /// </summary>
    public static Task<(RegulatoryApplicationId AppId, GlobalProductId GlobalProductId)>
        Ensure510kAsync(RegOSDbContext ctx)
        => EnsureAsync(ctx, Fda510k, "TEST-BLUEPRINT-FDA-510K");

    private static async Task<(RegulatoryApplicationId AppId, GlobalProductId GlobalProductId)>
        EnsureAsync(
            RegOSDbContext ctx,
            ApplicationTypeId applicationTypeId,
            string fixtureCode)
    {
        var existing = await FindAsync(ctx, fixtureCode);

        if (existing is not null)
            return existing.Value;

        var countryId = await ctx.Countries
            .AsNoTracking().Select(x => x.Id).FirstAsync();
        var organizationId = await ctx.Organizations
            .AsNoTracking().Select(x => x.Id).FirstAsync();
        var applicationType = await ctx.ApplicationTypes
            .AsNoTracking().SingleAsync(x => x.Id == applicationTypeId);

        var product = GlobalProduct.Register(
            TestTenant.Id, fixtureCode, "Blueprint Validation Product", ProductType.Drug);

        var application = RegulatoryApplicationAggregate.Create(
            TestTenant.Id,
            product.Id,
            countryId,
            Fda,
            applicationType,
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

            return await FindAsync(ctx, fixtureCode)
                ?? throw new InvalidOperationException(
                    "The FDA blueprint fixture could not be created or found.");
        }
    }

    private static async Task<(RegulatoryApplicationId, GlobalProductId)?> FindAsync(
        RegOSDbContext ctx,
        string fixtureCode)
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

        return applicationId == default ? null : (applicationId, globalProductId);
    }
}
