using Microsoft.EntityFrameworkCore;
using RegOS.Labeling.Domain.Aggregates.GlobalLabels;
using RegOS.Labeling.Domain.Aggregates.Indications;
using RegOS.Labeling.Domain.Aggregates.LocalLabels;
using RegOS.Product.Domain.Product;

using RegOS.SharedKernel.Abstractions;
using RegOS.SharedKernel.Primitives;

using RegulatoryApplicationAggregate =
    RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;
using CountryAggregate =
    RegOS.ReferenceData.Domain.Geography.Country.Country;
using AuthorityAggregate =
    RegOS.ReferenceData.Domain.Regulatory.Authority.Authority;
using OrganizationAggregate =
    RegOS.Organization.Domain.Aggregates.Organization.Organization;
using ApplicationTypeAggregate =
    RegOS.ReferenceData.Domain.ApplicationType.ApplicationType;
using SubmissionTypeAggregate =
    RegOS.ReferenceData.Domain.SubmissionType.SubmissionType;
using SubmissionSubTypeAggregate =
    RegOS.ReferenceData.Domain.SubmissionSubType.SubmissionSubType;
using SubmissionAggregate =
    RegOS.Submission.Domain.Submission.Submission;
using DocumentTypeAggregate =
    RegOS.ReferenceData.Domain.DocumentType.DocumentType;
using RegulatoryTemplateAggregate =
    RegOS.ReferenceData.Domain.Blueprint.RegulatoryTemplate;
using ProductDocumentAggregate =
    RegOS.ProductDocument.Domain.Aggregates.ProductDocument;
using RegistrationAggregate =
    RegOS.Registration.Domain.Aggregates.Registration.Registration;
using HaCorrespondenceAggregate =
    RegOS.Interaction.Domain.Correspondence.HaCorrespondence;
using CorrespondenceTypeAggregate =
    RegOS.ReferenceData.Domain.Regulatory.Correspondence.CorrespondenceType;
using AuthorityDivisionAggregate =
    RegOS.ReferenceData.Domain.Regulatory.Authority.AuthorityDivision;
using SubstanceAggregate =
    RegOS.ReferenceData.Domain.Substances.Substance;
using CommitmentAggregate =
    RegOS.Interaction.Domain.Commitments.Commitment;
using HaMeetingAggregate =
    RegOS.Interaction.Domain.Meetings.HaMeeting;
using InspectionAggregate =
    RegOS.Interaction.Domain.Inspections.Inspection;
using ClinicalStudyAggregate =
    RegOS.Study.Domain.Aggregates.ClinicalStudy.ClinicalStudy;
using NonClinicalStudyAggregate =
    RegOS.Study.Domain.Aggregates.NonClinicalStudy.NonClinicalStudy;
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

    public DbSet<GlobalProduct> Products =>
        Set<GlobalProduct>();

    /// <summary>
    /// The market-local tier: a product in one jurisdiction. Named
    /// <c>MedicinalProducts</c>, not <c>Products</c> — the two tiers are
    /// different aggregates in the same context.
    /// </summary>
    public DbSet<MedicinalProduct> MedicinalProducts =>
        Set<MedicinalProduct>();

    /// <summary>
    /// What a product physically is in one market. Screen word:
    /// <b>Presentation</b>. Its own root rather than a child of the market —
    /// composition and commerce move on different clocks (EPIC-010a S002).
    /// </summary>
    public DbSet<PharmaceuticalProductDetail> PharmaceuticalProductDetails =>
        Set<PharmaceuticalProductDetail>();

    /// <summary>
    /// What the patient physically receives — a vial, a pen, the kit holding
    /// them. Recursive through a nullable parent; the acyclicity is a domain
    /// rule, not a constraint here (EPIC-010a S004).
    /// </summary>
    public DbSet<MedicinalProductComponent> MedicinalProductComponents =>
        Set<MedicinalProductComponent>();

    /// <summary>
    /// A label held above any market — the core data sheet and its siblings
    /// (ADR-059).
    /// </summary>
    /// <remarks>
    /// <b>There is deliberately no <c>GlobalLabelVersions</c> set.</b> A version
    /// carries no <c>TenantId</c> and therefore has no query filter of its own,
    /// so <c>Set&lt;GlobalLabelVersion&gt;()</c> would read every tenant's
    /// issues. Every read of a version starts here, at the filtered root — the
    /// isolation lesson EPIC-010a's capstone paid for.
    /// </remarks>
    public DbSet<GlobalLabel> GlobalLabels =>
        Set<GlobalLabel>();

    /// <summary>
    /// A market's own controlled labelling document — what an authority
    /// approved, as against what the company holds centrally (ADR-059).
    /// </summary>
    /// <remarks>
    /// No <c>LocalLabelRevisions</c> set either, for the same reason: a
    /// revision carries no <c>TenantId</c> and has no filter of its own, so
    /// every read of one starts here.
    /// </remarks>
    public DbSet<LocalLabel> LocalLabels =>
        Set<LocalLabel>();

    /// <summary>
    /// What a product is approved to treat in one market — a regulatory fact,
    /// not an editorial artifact (ADR-059).
    /// </summary>
    /// <remarks>
    /// No set for populations, therapies or status entries: none carries a
    /// <c>TenantId</c>, so every read of one starts at this filtered root.
    /// </remarks>
    public DbSet<Indication> Indications =>
        Set<Indication>();

    public DbSet<RegulatoryApplicationAggregate> RegulatoryApplications =>
        Set<RegulatoryApplicationAggregate>();

    public DbSet<CountryAggregate> Countries =>
        Set<CountryAggregate>();

    public DbSet<AuthorityAggregate> Authorities =>
        Set<AuthorityAggregate>();

    public DbSet<OrganizationAggregate> Organizations =>
        Set<OrganizationAggregate>();

    public DbSet<RegOS.Organization.Domain.Aggregates.OrganizationSite.OrganizationSite>
        OrganizationSites =>
        Set<RegOS.Organization.Domain.Aggregates.OrganizationSite.OrganizationSite>();

    public DbSet<RegOS.ReferenceData.Domain.Organization.IdentifierScheme>
        IdentifierSchemes =>
        Set<RegOS.ReferenceData.Domain.Organization.IdentifierScheme>();

    public DbSet<RegOS.Organization.Domain.Aggregates.OrganizationDivision.OrganizationDivision>
        OrganizationDivisions =>
        Set<RegOS.Organization.Domain.Aggregates.OrganizationDivision.OrganizationDivision>();

    public DbSet<RegOS.Organization.Domain.Aggregates.Contact.Contact> Contacts =>
        Set<RegOS.Organization.Domain.Aggregates.Contact.Contact>();

    public DbSet<RegOS.ReferenceData.Domain.Organization.ContactRole>
        ContactRoles =>
        Set<RegOS.ReferenceData.Domain.Organization.ContactRole>();

    public DbSet<ApplicationTypeAggregate> ApplicationTypes =>
        Set<ApplicationTypeAggregate>();

    /// <summary>
    /// What a regulatory activity is — eCTD's <c>submission-type</c>. Global
    /// world fact, so no tenant filter, like the catalogue above it.
    /// </summary>
    public DbSet<SubmissionTypeAggregate> SubmissionTypes =>
        Set<SubmissionTypeAggregate>();

    /// <summary>
    /// What a sequence does to its activity — eCTD's
    /// <c>submission-sub-type</c>. An independent axis, not a taxonomy beneath
    /// <see cref="SubmissionTypes"/> (ADR-047 §6).
    /// </summary>
    public DbSet<SubmissionSubTypeAggregate> SubmissionSubTypes =>
        Set<SubmissionSubTypeAggregate>();

    public DbSet<SubmissionAggregate> Submissions =>
        Set<SubmissionAggregate>();

    /// <summary>
    /// Correspondence with a health authority. Tenant-owned and fail-closed,
    /// like every other record of what the business did (ADR-031).
    /// </summary>
    public DbSet<HaCorrespondenceAggregate> HaCorrespondence =>
        Set<HaCorrespondenceAggregate>();

    /// <summary>
    /// The correspondence vocabulary. A global world fact, so no filter —
    /// the third of ADR-038's three filter shapes.
    /// </summary>
    public DbSet<CorrespondenceTypeAggregate> CorrespondenceTypes =>
        Set<CorrespondenceTypeAggregate>();

    /// <summary>
    /// Units inside a health authority. Platform-seeded, tenant-augmentable —
    /// RegOS has no authoritative source for the world's authority divisions,
    /// so a tenant records the ones it actually deals with.
    /// </summary>
    public DbSet<AuthorityDivisionAggregate> AuthorityDivisions =>
        Set<AuthorityDivisionAggregate>();

    /// <summary>
    /// Scientific entities that exist independently of any product. Shared
    /// catalogue plus a tenant's own — an innovator holds a compound before it
    /// is in anyone's registry (ADR-058).
    /// </summary>
    public DbSet<SubstanceAggregate> Substances =>
        Set<SubstanceAggregate>();

    /// <summary>
    /// What we promised an authority. Tenant-owned and fail-closed.
    /// </summary>
    public DbSet<CommitmentAggregate> Commitments =>
        Set<CommitmentAggregate>();

    /// <summary>Meetings with an authority. Tenant-owned and fail-closed.</summary>
    public DbSet<HaMeetingAggregate> HaMeetings =>
        Set<HaMeetingAggregate>();

    /// <summary>Authority inspections. Tenant-owned and fail-closed.</summary>
    public DbSet<InspectionAggregate> Inspections =>
        Set<InspectionAggregate>();

    public DbSet<RegistrationAggregate> Registrations =>
        Set<RegistrationAggregate>();

    /// <summary>
    /// Studies in human subjects. Tenant-owned and fail-closed.
    /// </summary>
    /// <remarks>
    /// Two sets rather than one with a discriminator, because they are two
    /// aggregates (ADR-056). A read that wants both composes them.
    /// </remarks>
    public DbSet<ClinicalStudyAggregate> ClinicalStudies =>
        Set<ClinicalStudyAggregate>();

    /// <summary>
    /// Studies not in human subjects — the Module 4 half. Tenant-owned and
    /// fail-closed.
    /// </summary>
    public DbSet<NonClinicalStudyAggregate> NonClinicalStudies =>
        Set<NonClinicalStudyAggregate>();

    public DbSet<DocumentTypeAggregate> DocumentTypes =>
        Set<DocumentTypeAggregate>();

    public DbSet<RegulatoryTemplateAggregate> RegulatoryTemplates =>
        Set<RegulatoryTemplateAggregate>();

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
    /// Tenant filtering has <b>three shapes</b>, and choosing between them is
    /// the decision every new entity faces:
    /// <list type="number">
    /// <item><b>Fail-closed tenant-owned</b> — <c>x.TenantId == CurrentTenant</c>.
    /// The tenant owns the data. <c>Users</c>, <c>Products</c>,
    /// <c>MedicinalProducts</c>, <c>PharmaceuticalProductDetails</c>,
    /// <c>MedicinalProductComponents</c>,
    /// <c>RegulatoryApplications</c>, <c>Submissions</c>, <c>ProductDocuments</c>,
    /// <c>Registrations</c>, <c>Organizations</c>, <c>OrganizationSites</c>,
    /// <c>Contacts</c>, <c>OrganizationDivisions</c>.</item>
    /// <item><b>Shared plus extensible</b> — <c>TenantId == null || == CurrentTenant</c>.
    /// The platform ships a baseline the tenant may extend.
    /// <c>DocumentTypes</c>, <c>RegulatoryTemplates</c>, <c>ContactRoles</c>,
    /// <c>AuthorityDivisions</c>, <c>Substances</c>.</item>
    /// <item><b>Global world facts</b> — no filter. RegOS is describing an
    /// external reality that does not differ by tenant. <c>Countries</c>,
    /// <c>Authorities</c>, <c>ApplicationTypes</c>, <c>IdentifierSchemes</c>.</item>
    /// </list>
    /// Also unfiltered, for different reasons: <c>Tenants</c> (the platform
    /// tier), and the person-scoped satellites (<c>UserCredentials</c>,
    /// <c>RefreshTokens</c>, <c>Invitations</c>, <c>PasswordResets</c>,
    /// <c>Sessions</c>), which carry no tenant and are reachable only by user
    /// id or token hash. Child entities (<c>SubmissionDocuments</c>,
    /// <c>DocumentVersions</c>, <c>SubmissionDeletions</c>,
    /// <c>RegistrationStatusHistory</c>, <c>SiteIdentifiers</c>,
    /// <c>ContactRoleAssignments</c>, <c>ContactEmails</c>, <c>ContactPhones</c>,
    /// <c>OrganizationIdentifiers</c>)
    /// are reachable
    /// only through a filtered root.
    /// <para>
    /// <c>Organizations</c> was listed here as an unfiltered global directory
    /// until ADR-032 made it tenant-owned; the list above is the corrected one.
    /// </para>
    /// </para>
    /// </remarks>
    private void ApplyTenantFilters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAggregate>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        modelBuilder.Entity<GlobalProduct>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        // The market-local tier carries its own tenant rather than inheriting
        // the global product's: it is a root, loaded and queried directly, and
        // a filter that reached through a parent would not apply to the
        // registration joins that are its hottest path.
        modelBuilder.Entity<MedicinalProduct>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        modelBuilder.Entity<PharmaceuticalProductDetail>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        modelBuilder.Entity<MedicinalProductComponent>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        // A label is tenant-owned, so it takes the first of ADR-038's three
        // filter shapes. Its versions carry no TenantId and are reachable only
        // through it (ADR-059).
        modelBuilder.Entity<GlobalLabel>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        modelBuilder.Entity<LocalLabel>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        modelBuilder.Entity<Indication>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        modelBuilder.Entity<RegulatoryApplicationAggregate>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        modelBuilder.Entity<SubmissionAggregate>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        modelBuilder.Entity<ProductDocumentAggregate>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        modelBuilder.Entity<HaCorrespondenceAggregate>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        modelBuilder.Entity<CommitmentAggregate>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        modelBuilder.Entity<HaMeetingAggregate>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        modelBuilder.Entity<InspectionAggregate>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        // A sponsor's studies are the sponsor's. Both kinds are roots with
        // their own TenantId rather than reaching through anything (ADR-056),
        // so both take the ordinary first filter shape.
        modelBuilder.Entity<ClinicalStudyAggregate>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        modelBuilder.Entity<NonClinicalStudyAggregate>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        // Platform-seeded, tenant-augmentable: the second of ADR-038's three
        // filter shapes, the same one ContactRole takes. The argument differs
        // though — a tenant is not extending what the FDA means, it is
        // recording which of its divisions they correspond with, because RegOS
        // cannot ship a complete universe of them.
        modelBuilder.Entity<AuthorityDivisionAggregate>().HasQueryFilter(
            x => CurrentTenant != null
                && (x.TenantId == null || x.TenantId == CurrentTenant));

        // The same shape again, and a third distinct argument for it: an
        // authoritative substance registry *does* exist, and the tenant's
        // molecule is simply not in it yet. That reason resolves itself when
        // licensed terminology arrives; AuthorityDivision's never does
        // (ADR-058 §2).
        modelBuilder.Entity<SubstanceAggregate>().HasQueryFilter(
            x => CurrentTenant != null
                && (x.TenantId == null || x.TenantId == CurrentTenant));

        modelBuilder.Entity<RegistrationAggregate>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        // A tenant's registry of business relationships — even the *names*
        // in another tenant's registry are competitively sensitive, so the
        // directory that started global under the fused model is tenant-owned
        // since ADR-032.
        modelBuilder.Entity<OrganizationAggregate>().HasQueryFilter(
            x => CurrentTenant != null && x.TenantId == CurrentTenant);

        // A site is a ROOT, reachable directly through the tenant-wide
        // directory rather than only through its organization — so it carries
        // its own tenant and its own filter instead of inheriting the parent's.
        // Site addresses are exactly the competitively sensitive data ADR-032
        // was written for.
        modelBuilder.Entity<
            RegOS.Organization.Domain.Aggregates.OrganizationSite.OrganizationSite>()
            .HasQueryFilter(
                x => CurrentTenant != null && x.TenantId == CurrentTenant);

        // A named person at a partner or an authority — a root reachable
        // through the contact directory, so it carries its own filter for the
        // same reason a site does.
        modelBuilder.Entity<RegOS.Organization.Domain.Aggregates.Contact.Contact>()
            .HasQueryFilter(
                x => CurrentTenant != null && x.TenantId == CurrentTenant);

        // A root by stable identity rather than by an immediate directory —
        // EPIC-006 will reference *this division* by id. Same filter shape.
        modelBuilder.Entity<
            RegOS.Organization.Domain.Aggregates.OrganizationDivision.OrganizationDivision>()
            .HasQueryFilter(
                x => CurrentTenant != null && x.TenantId == CurrentTenant);

        // System types (null tenant) are visible to every authenticated
        // tenant; a tenant's own extensions only to that tenant.
        modelBuilder.Entity<DocumentTypeAggregate>().HasQueryFilter(
            x => CurrentTenant != null
                && (x.TenantId == null || x.TenantId == CurrentTenant));

        // Shared blueprints (null tenant) are visible to every authenticated
        // tenant; a tenant's own templates only to that tenant — the same
        // shared-plus-extensible shape as DocumentType.
        modelBuilder.Entity<RegulatoryTemplateAggregate>().HasQueryFilter(
            x => CurrentTenant != null
                && (x.TenantId == null || x.TenantId == CurrentTenant));

        // Roles mix legislated vocabulary ("Qualified Person") with a company's
        // own words ("APAC Regulatory Lead"), so the platform ships a baseline
        // and a tenant extends it — the DocumentType shape, not the Country one.
        modelBuilder.Entity<RegOS.ReferenceData.Domain.Organization.ContactRole>()
            .HasQueryFilter(
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
