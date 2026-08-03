using Microsoft.EntityFrameworkCore;

using RegOS.Organization.Domain.Aggregates.Contact;
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
        await EnsureApplicantIsIdentifiableAsync(ctx);

        var existing = await FindAsync(ctx, fixtureCode);

        if (existing is not null)
        {
            // Find-or-create, so the row is usually already there — and it was
            // created before an application number could be recorded at all.
            await EnsureNumberedAsync(ctx, existing.Value.Item1);

            return existing.Value;
        }

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

        // Every FDA sequence is filed against a number the authority assigned.
        // Six digits, because that is what FDA issues — the shape is checked at
        // the FDA boundary, not by the aggregate (ADR-055).
        application.RecordApplicationNumber("123456");

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

    /// <summary>
    /// The number FDA assigned. Six digits, because that is what FDA issues —
    /// and the shape is checked at the FDA boundary rather than by the
    /// aggregate (ADR-055).
    /// </summary>
    private static async Task EnsureNumberedAsync(
        RegOSDbContext ctx, RegulatoryApplicationId applicationId)
    {
        var application = await ctx.RegulatoryApplications
            .SingleAsync(x => x.Id == applicationId);

        if (application.ApplicationNumber is not null)
            return;

        application.RecordApplicationNumber("123456");

        // Parallel test classes converge on this one row, so two of them can
        // both find it unnumbered. Both write the same value, so losing the
        // race costs nothing — the same reasoning the create path already uses.
        try
        {
            await ctx.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
        }
        finally
        {
            ctx.ChangeTracker.Clear();
        }
    }

    /// <summary>
    /// FDA identifies the applicant by a DUNS number, and the fixture's
    /// applicant is whichever organization the seed created first.
    /// </summary>
    /// <remarks>
    /// <b>Given rather than defaulted.</b> The generator refuses a missing DUNS
    /// rather than writing FDA's <c>999999999</c>, because the placeholder is
    /// permitted *"if you are unable to acquire a DUNS number"* — a fact about
    /// the applicant that an empty column does not establish (E25). So the
    /// fixture has to supply one, exactly as a user would.
    /// </remarks>
    private static async Task EnsureApplicantIsIdentifiableAsync(RegOSDbContext ctx)
    {
        var organizationId = await ctx.Organizations
            .AsNoTracking().Select(x => x.Id).FirstAsync();

        var duns = await ctx.IdentifierSchemes
            .AsNoTracking()
            .Where(x => x.Code == "DUNS")
            .Select(x => x.Id)
            .SingleAsync();

        var organization = await ctx.Organizations
            .Include(x => x.Identifiers)
            .SingleAsync(x => x.Id == organizationId);

        if (organization.Identifiers.Any(x => x.SchemeId == duns))
            return;

        // A fictional nine-digit number. Real ones are issued by Dun &
        // Bradstreet and this is a test database.
        organization.AddIdentifier(duns, "123456789");

        // As above: a lost race means someone else recorded the same number.
        try
        {
            await ctx.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
        }
        finally
        {
            ctx.ChangeTracker.Clear();
        }
    }
}

