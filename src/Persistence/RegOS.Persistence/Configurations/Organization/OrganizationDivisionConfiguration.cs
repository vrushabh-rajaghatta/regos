using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Organization.Domain.Aggregates.OrganizationDivision;
using RegOS.SharedKernel.Primitives;

using DivisionAggregate = RegOS.Organization.Domain.Aggregates.OrganizationDivision.OrganizationDivision;
using OrganizationAggregate = RegOS.Organization.Domain.Aggregates.Organization.Organization;

namespace RegOS.Persistence.Configurations.Organization;

public sealed class OrganizationDivisionConfiguration
    : IEntityTypeConfiguration<DivisionAggregate>
{
    public void Configure(EntityTypeBuilder<DivisionAggregate> builder)
    {
        builder.ToTable("OrganizationDivisions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new OrganizationDivisionId(value));

        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.HasIndex(x => x.TenantId);

        builder.Property(x => x.OrganizationId)
            .HasConversion(id => id.Value, value => new OrganizationId(value))
            .IsRequired();

        builder.HasOne<OrganizationAggregate>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.OrganizationId);

        builder.Property(x => x.Name)
            .HasMaxLength(DivisionAggregate.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Acronym).HasMaxLength(50);

        builder.Property(x => x.Status).HasConversion<int>().IsRequired();

        builder.Property(x => x.StatusDate)
            .HasColumnType("date")
            .IsRequired();
    }
}

public sealed class OrganizationIdentifierConfiguration
    : IEntityTypeConfiguration<OrganizationIdentifier>
{
    public void Configure(EntityTypeBuilder<OrganizationIdentifier> builder)
    {
        builder.ToTable("OrganizationIdentifiers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new OrganizationIdentifierId(value));

        builder.Property(x => x.SchemeId)
            .HasConversion(
                id => id.Value,
                value => new RegOS.ReferenceData.Domain.Organization
                    .IdentifierSchemeId(value))
            .IsRequired();

        builder.HasOne<RegOS.ReferenceData.Domain.Organization.IdentifierScheme>()
            .WithMany()
            .HasForeignKey(x => x.SchemeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Value)
            .HasMaxLength(OrganizationIdentifier.ValueMaxLength)
            .IsRequired();

        builder.Property<OrganizationId>("OrganizationId")
            .HasConversion(id => id.Value, value => new OrganizationId(value));

        // The aggregate's rule in the database too: one identifier per scheme.
        builder.HasIndex("OrganizationId", nameof(OrganizationIdentifier.SchemeId))
            .IsUnique();
    }
}
