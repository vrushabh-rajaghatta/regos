using Microsoft.EntityFrameworkCore;

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

namespace RegOS.Persistence;

public sealed class RegOSDbContext : DbContext
{
    public RegOSDbContext(
        DbContextOptions<RegOSDbContext> options)
        : base(options)
    {
    }

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

    public DbSet<UserAggregate> Users =>
        Set<UserAggregate>();

    /// <summary>Read-only projection over Users for the user directory.</summary>
    public DbSet<ReadModels.UserDirectoryRow> UserDirectory =>
        Set<ReadModels.UserDirectoryRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(RegOSDbContext).Assembly);
    }
}
