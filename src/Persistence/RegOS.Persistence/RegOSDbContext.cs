using Microsoft.EntityFrameworkCore;

using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Primitives;

using ProductAggregate = RegOS.Product.Domain.Product.Product;
using RegulatoryApplicationAggregate =
    RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;
using CountryAggregate =
    RegOS.ReferenceData.Domain.Geography.Country.Country;
using AuthorityAggregate =
    RegOS.ReferenceData.Domain.Regulatory.Authority.Authority;
using OrganizationAggregate =
    RegOS.Organization.Domain.Aggregates.Organization.Organization;
using SubmissionTypeAggregate =
    RegOS.ReferenceData.Domain.SubmissionType.SubmissionType;
using SubmissionAggregate =
    RegOS.Submission.Domain.Submission.Submission;
using SubmissionSnapshotAggregate =
    RegOS.Submission.Domain.Snapshot.SubmissionSnapshot;
using DocumentTypeAggregate =
    RegOS.ReferenceData.Domain.DocumentType.DocumentType;
using ProductDocumentAggregate =
    RegOS.ProductDocument.Domain.Aggregates.ProductDocument;
using UserAggregate =
    RegOS.Platform.Domain.Aggregates.User.User;
using TenantAggregate =
    RegOS.Platform.Domain.Aggregates.Tenant.Tenant;
using UserCredentialAggregate =
    RegOS.Platform.Domain.Aggregates.UserCredential.UserCredential;
using RefreshTokenAggregate =
    RegOS.Platform.Domain.Aggregates.RefreshToken.RefreshToken;
using InvitationAggregate =
    RegOS.Platform.Domain.Aggregates.Invitation.Invitation;
using PasswordResetAggregate =
    RegOS.Platform.Domain.Aggregates.PasswordReset.PasswordReset;
using SessionAggregate =
    RegOS.Platform.Domain.Aggregates.Session.Session;

namespace RegOS.Persistence;

public sealed class RegOSDbContext : DbContext
{
    private readonly ITenantContext? _tenantContext;

    /// <summary>
    /// The tenant context is optional so the context can exist before a
    /// request does (design-time tools, direct construction in tests). The
    /// filters treat a missing context exactly like a missing identity:
    /// no tenant, no rows. Fail closed, never open.
    /// </summary>
    public RegOSDbContext(
        DbContextOptions<RegOSDbContext> options,
        ITenantContext? tenantContext = null)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Read per query execution, not captured at model build: the filter
    /// expressions reference this instance member, which EF lifts to a SQL
    /// parameter — so the compiled model is shared while the tenant is always
    /// the current request's. Never <c>Guid.Empty</c>: a strongly typed id
    /// cannot hold it, so "no tenant" is null and only null.
    /// </summary>
    private TenantId? CurrentTenant =>
        _tenantContext?.TenantIdOrNull;

    private Guid? CurrentTenantGuid =>
        _tenantContext?.TenantIdOrNull is { } tenant
            ? tenant.Value
            : null;

    public DbSet<ProductAggregate> Products =>
        Set<ProductAggregate>();

    public DbSet<RegulatoryApplicationAggregate> RegulatoryApplications =>
        Set<RegulatoryApplicationAggregate>();

    public DbSet<CountryAggregate> Countries =>
        Set<CountryAggregate>();

    public DbSet<AuthorityAggregate> Authorities =>
        Set<AuthorityAggregate>();

    public DbSet<OrganizationAggregate> Organizations =>
        Set<OrganizationAggregate>();

    public DbSet<SubmissionTypeAggregate> SubmissionTypes =>
        Set<SubmissionTypeAggregate>();

    public DbSet<SubmissionAggregate> Submissions =>
        Set<SubmissionAggregate>();

    public DbSet<SubmissionSnapshotAggregate> SubmissionSnapshots =>
        Set<SubmissionSnapshotAggregate>();

    public DbSet<DocumentTypeAggregate> DocumentTypes =>
        Set<DocumentTypeAggregate>();

    public DbSet<ProductDocumentAggregate> ProductDocuments =>
        Set<ProductDocumentAggregate>();

    public DbSet<TenantAggregate> Tenants =>
        Set<TenantAggregate>();

    public DbSet<UserAggregate> Users =>
        Set<UserAggregate>();

    public DbSet<UserCredentialAggregate> UserCredentials =>
        Set<UserCredentialAggregate>();

    public DbSet<RefreshTokenAggregate> RefreshTokens =>
        Set<RefreshTokenAggregate>();

    public DbSet<InvitationAggregate> Invitations =>
        Set<InvitationAggregate>();

    public DbSet<PasswordResetAggregate> PasswordResets =>
        Set<PasswordResetAggregate>();

    public DbSet<SessionAggregate> Sessions =>
        Set<SessionAggregate>();

    /// <summary>Read-only projection over Users for the user directory.</summary>
    public DbSet<ReadModels.UserDirectoryRow> UserDirectory =>
        Set<ReadModels.UserDirectoryRow>();

    /// <summary>Read-only projection over Products for the product directory.</summary>
    public DbSet<ReadModels.ProductDirectoryRow> ProductDirectory =>
        Set<ReadModels.ProductDirectoryRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(RegOSDbContext).Assembly);

        ApplyTenantFilters(modelBuilder);
    }

    /// <summary>
    /// Tenant isolation, enforced once, here, for every tenant-owned entity
    /// (ADR-031). A handler that forgets its <c>.Where</c> now returns the
    /// caller's rows instead of everyone's.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every filter starts with an explicit null guard rather than relying on
    /// SQL null semantics. The distinction matters for <c>Users</c>: with a
    /// null tenant, a bare <c>u.TenantId == CurrentTenant</c> would translate
    /// to <c>"TenantId" IS NULL</c> — which matches every platform user. The
    /// guard makes "no identity" mean <em>no rows</em>, not "the null-tenant
    /// rows".
    /// </para>
    /// <para>
    /// Deliberately unfiltered, by tier (ADR-031): <c>Tenants</c> and
    /// <c>Organizations</c> (global directories), <c>Countries</c>,
    /// <c>Authorities</c>, <c>SubmissionTypes</c> (world facts), and the
    /// person-scoped satellites (<c>UserCredentials</c>, <c>RefreshTokens</c>,
    /// <c>Invitations</c>, <c>PasswordResets</c>, <c>Sessions</c>), which
    /// carry no tenant and are reachable only by user id or token hash.
    /// Child entities (<c>SubmissionDocuments</c>, <c>DocumentVersions</c>,
    /// <c>SnapshotDocuments</c>) are reachable only through a filtered root.
    /// </para>
    /// </remarks>
    private void ApplyTenantFilters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAggregate>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        modelBuilder.Entity<ProductAggregate>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        modelBuilder.Entity<RegulatoryApplicationAggregate>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        modelBuilder.Entity<SubmissionAggregate>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        modelBuilder.Entity<SubmissionSnapshotAggregate>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        modelBuilder.Entity<ProductDocumentAggregate>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        // A tenant's registry of business relationships — even the *names*
        // in another tenant's registry are competitively sensitive, so the
        // directory that started global under the fused model is tenant-owned
        // since ADR-032.
        modelBuilder.Entity<OrganizationAggregate>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        // System types (null tenant) are visible to every authenticated
        // tenant; a tenant's own extensions only to that tenant.
        modelBuilder.Entity<DocumentTypeAggregate>().HasQueryFilter(
            x => CurrentTenant != null
                && (x.TenantId == null || x.TenantId == CurrentTenant));

        // The ToView read models map onto the same physical tables but are
        // different CLR types — the aggregate filters above do NOT propagate
        // to them. Left unfiltered they would be a ready-made leak path.
        modelBuilder.Entity<ReadModels.UserDirectoryRow>().HasQueryFilter(
            x => CurrentTenantGuid != null && x.TenantId == CurrentTenantGuid);

        modelBuilder.Entity<ReadModels.ProductDirectoryRow>().HasQueryFilter(
            x => CurrentTenantGuid != null && x.TenantId == CurrentTenantGuid);
    }
}
