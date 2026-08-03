using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Organization.Domain.Aggregates.Contact;
using RegOS.Organization.Domain.Aggregates.Organization;
using RegOS.Organization.Domain.Aggregates.OrganizationSite;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Organization;
using RegOS.SharedKernel.Primitives;

using ContactAggregate = RegOS.Organization.Domain.Aggregates.Contact.Contact;
using OrganizationAggregate = RegOS.Organization.Domain.Aggregates.Organization.Organization;

namespace RegOS.Persistence.Configurations.Organization;

public sealed class ContactConfiguration
    : IEntityTypeConfiguration<ContactAggregate>
{
    public void Configure(EntityTypeBuilder<ContactAggregate> builder)
    {
        builder.ToTable("Contacts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ContactId(value));

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

        // Optional: a partner's head-office regulatory lead has no site, and an
        // authority reviewer certainly does not.
        builder.Property(x => x.OrganizationSiteId)
            .HasConversion(
                id => id!.Value,
                value => new OrganizationSiteId(value));

        builder.HasOne<OrganizationSite>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationSiteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CountryId)
            .HasConversion(
                id => id!.Value.Value,
                value => new CountryId(value));

        builder.HasOne<Country>()
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.FirstName)
            .HasMaxLength(ContactAggregate.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(ContactAggregate.NameMaxLength)
            .IsRequired();

        builder.HasIndex(x => x.LastName);

        builder.Property(x => x.Title).HasMaxLength(150);
        builder.Property(x => x.Department).HasMaxLength(150);

        builder.Property(x => x.Status).HasConversion<int>().IsRequired();

        builder.Property(x => x.StatusDate)
            .HasColumnType("date")
            .IsRequired();

        Owned(builder, x => x.Roles, nameof(ContactAggregate.Roles));
        Owned(builder, x => x.Emails, nameof(ContactAggregate.Emails));
        Owned(builder, x => x.Phones, nameof(ContactAggregate.Phones));
    }

    // Each child holds no FK property, so EF uses a shadow "ContactId".
    private static void Owned<TChild>(
        EntityTypeBuilder<ContactAggregate> builder,
        System.Linq.Expressions.Expression<
            Func<ContactAggregate, IEnumerable<TChild>?>> navigation,
        string name)
        where TChild : class
    {
        builder.HasMany(navigation)
            .WithOne()
            .HasForeignKey("ContactId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(name)!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class ContactRoleAssignmentConfiguration
    : IEntityTypeConfiguration<ContactRoleAssignment>
{
    public void Configure(EntityTypeBuilder<ContactRoleAssignment> builder)
    {
        builder.ToTable("ContactRoleAssignments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new ContactRoleAssignmentId(value));

        builder.Property(x => x.RoleId)
            .HasConversion(id => id.Value, value => new ContactRoleId(value))
            .IsRequired();

        builder.HasOne<ContactRole>()
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property<ContactId>("ContactId")
            .HasConversion(id => id.Value, value => new ContactId(value));

        // The aggregate's rule, enforced by the database too: holding a role
        // twice would say the same thing twice.
        builder.HasIndex("ContactId", nameof(ContactRoleAssignment.RoleId))
            .IsUnique();

        // "Who is the QP?" scans by role, across the registry.
        builder.HasIndex(x => x.RoleId);
    }
}

public sealed class ContactEmailConfiguration
    : IEntityTypeConfiguration<ContactEmail>
{
    public void Configure(EntityTypeBuilder<ContactEmail> builder)
    {
        builder.ToTable("ContactEmails");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ContactEmailId(value));

        builder.Property(x => x.Address)
            .HasMaxLength(ContactAggregate.EmailMaxLength)
            .IsRequired();

        builder.Property<ContactId>("ContactId")
            .HasConversion(id => id.Value, value => new ContactId(value));

        builder.HasIndex("ContactId");
    }
}

public sealed class ContactPhoneConfiguration
    : IEntityTypeConfiguration<ContactPhone>
{
    public void Configure(EntityTypeBuilder<ContactPhone> builder)
    {
        builder.ToTable("ContactPhones");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new ContactPhoneId(value));

        builder.Property(x => x.Number)
            .HasMaxLength(ContactAggregate.PhoneMaxLength)
            .IsRequired();

        // Stored as its name, not its ordinal: a column reading 'Mobile' says
        // what it means to anyone reading the database, and reordering the enum
        // cannot silently change what existing rows mean.
        builder.Property(x => x.Kind)
            .HasConversion<string>()
            .HasMaxLength(16);

        builder.Property<ContactId>("ContactId")
            .HasConversion(id => id.Value, value => new ContactId(value));

        builder.HasIndex("ContactId");
    }
}
