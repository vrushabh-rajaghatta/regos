using Microsoft.EntityFrameworkCore;

using ProductAggregate = RegOS.Product.Domain.Product.Product;
using RegulatoryApplicationAggregate =
    RegOS.RegulatoryApplication.Domain.Aggregates.RegulatoryApplication.RegulatoryApplication;
using CountryAggregate =
    RegOS.MasterData.Domain.Geography.Country.Country;
using AuthorityAggregate =
    RegOS.MasterData.Domain.Regulatory.Authority.Authority;
using OrganizationAggregate =
    RegOS.Organization.Domain.Aggregates.Organization.Organization;
using SubmissionTypeAggregate =
    RegOS.ReferenceData.Domain.SubmissionType.SubmissionType;
using SubmissionAggregate =
    RegOS.Submission.Domain.Submission.Submission;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(RegOSDbContext).Assembly);
    }
}
