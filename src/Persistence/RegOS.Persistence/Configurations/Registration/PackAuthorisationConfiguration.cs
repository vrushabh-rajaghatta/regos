using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RegOS.Product.Domain.Product;
using RegOS.Registration.Domain.Aggregates.PackAuthorisations;
using RegOS.Registration.Domain.Aggregates.Registration;
using RegOS.SharedKernel.Primitives;

using RegistrationAggregate =
    RegOS.Registration.Domain.Aggregates.Registration.Registration;

namespace RegOS.Persistence.Configurations.Registration;

public sealed class PackAuthorisationConfiguration
    : IEntityTypeConfiguration<PackAuthorisation>
{
    public void Configure(EntityTypeBuilder<PackAuthorisation> builder)
    {
        builder.ToTable("PackAuthorisations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new PackAuthorisationId(value))
            .ValueGeneratedNever();

        // The owning tenant (ADR-031), held by value; no FK to Tenants.
        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => new TenantId(value))
            .IsRequired();

        builder.Property(x => x.RegistrationId)
            .HasConversion(id => id.Value, value => new RegistrationId(value))
            .IsRequired();

        builder.Property(x => x.PackagedProductId)
            .HasConversion(id => id.Value, value => new PackagedProductId(value))
            .IsRequired();

        builder.Property(x => x.AuthorisedOn)
            .IsRequired();

        builder.Property(x => x.RecordedOnUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Cascade from both sides, and for the same reason each time: an
        // authorisation is a statement *about* a licence and a pack, and it
        // means nothing once either is gone. This is not the artwork case one
        // context over, where the approved document is a fact in its own right
        // and survives its pack losing the link.
        builder.HasOne<RegistrationAggregate>()
            .WithMany()
            .HasForeignKey(x => x.RegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<PackagedProduct>()
            .WithMany()
            .HasForeignKey(x => x.PackagedProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // One statement per (licence, pack), enforced where a race cannot slip
        // past the handler's own check. Deliberately *not* unique on the pack
        // alone: a pack authorised under two licences is legitimate — a partial
        // divestment leaves exactly that.
        builder.HasIndex(x => new { x.RegistrationId, x.PackagedProductId })
            .IsUnique();

        // "Which packs are authorised in this market?" walks in from the pack
        // side, so this is the index the capstone read goes through.
        builder.HasIndex(x => x.PackagedProductId);

        builder.HasIndex(x => x.TenantId);
    }
}
