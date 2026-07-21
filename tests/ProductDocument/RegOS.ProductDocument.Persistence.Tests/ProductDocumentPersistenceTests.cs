using RegOS.SharedKernel.Primitives;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.Entities;
using RegOS.ProductDocument.Domain.Enums;
using RegOS.ProductDocument.Domain.IDs;
using RegOS.ProductDocument.Infrastructure.Repositories;
using RegOS.ReferenceData.Domain.DocumentType;

using ProductDocumentAggregate =
    RegOS.ProductDocument.Domain.Aggregates.ProductDocument;

namespace RegOS.ProductDocument.Persistence.Tests;

// Integration test — exercises the real EF mapping against the dev Postgres.
// Requires the database to be running (docker postgres-local).
public class ProductDocumentPersistenceTests
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=regos;Username=admin;Password=password123";

    // Seeded system document type (CER) from Milestone 18.2.
    private static readonly DocumentTypeId SeededCer =
        new(Guid.Parse("50000000-0000-0000-0000-000000000001"));

    private static DbContextOptions<RegOSDbContext> Options() =>
        new DbContextOptionsBuilder<RegOSDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

    [Fact]
    public async Task Saves_reloads_and_cascade_deletes_a_document_with_its_version()
    {
        ProductId productId;
        ProductDocumentId documentId;

        // --- Save: new document + initial version, via the repository. ---
        await using (var ctx = new RegOSDbContext(Options()))
        {
            productId = await ctx.Products.Select(p => p.Id).FirstAsync();

            var document = ProductDocumentAggregate.Create(TenantId.New(), 
                productId,
                SeededCer,
                "Persistence Verify " + Guid.NewGuid());

            document.AddInitialVersion(
                originalFileName: "cer.pdf",
                storedFileName: "stored-cer-v1.pdf",
                contentType: "application/pdf",
                fileSize: 1024,
                storagePath: "products/p/cer-v1.pdf",
                checksum: "sha256-v1");

            documentId = document.Id;

            var repository = new ProductDocumentRepository(ctx);
            await repository.AddAsync(document, default);
        }

        // --- Reload from a fresh context and verify the aggregate. ---
        await using (var ctx = new RegOSDbContext(Options()))
        {
            var repository = new ProductDocumentRepository(ctx);
            var reloaded = await repository.GetByIdAsync(documentId, default);

            reloaded.Should().NotBeNull();
            reloaded!.ProductId.Should().Be(productId);
            reloaded.DocumentTypeId.Should().Be(SeededCer);
            reloaded.Status.Should().Be(ProductDocumentStatus.Draft);

            reloaded.Versions.Should().ContainSingle();
            var version = reloaded.Versions.Single();
            version.VersionNumber.Should().Be(1);
            version.OriginalFileName.Should().Be("cer.pdf");
            version.FileSize.Should().Be(1024);
            version.StoragePath.Should().Be("products/p/cer-v1.pdf");

            reloaded.CurrentVersionId.Should().Be(version.Id);
        }

        // --- Cascade: deleting the document removes its versions. ---
        await using (var ctx = new RegOSDbContext(Options()))
        {
            var document = await ctx.ProductDocuments
                .Include(x => x.Versions)
                .FirstAsync(x => x.Id == documentId);

            ctx.ProductDocuments.Remove(document);
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = new RegOSDbContext(Options()))
        {
            var documentExists = await ctx.ProductDocuments
                .AnyAsync(x => x.Id == documentId);
            documentExists.Should().BeFalse();

            var orphanVersionCount = await ctx.Set<DocumentVersion>()
                .CountAsync(v =>
                    EF.Property<ProductDocumentId>(v, "ProductDocumentId") == documentId);
            orphanVersionCount.Should().Be(0);
        }
    }
}
