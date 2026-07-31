using RegOS.SharedKernel.Primitives;
using RegOS.Product.Domain.Product;
using RegOS.ReferenceData.Domain.DocumentType;

using ProductDocumentAggregate =
    RegOS.ProductDocument.Domain.Aggregates.ProductDocument;

namespace RegOS.ProductDocument.Domain.Tests;

internal static class TestFactory
{
    public static ProductDocumentAggregate NewDocument() =>
        ProductDocumentAggregate.Create(TenantId.New(), 
            new GlobalProductId(Guid.NewGuid()),
            new DocumentTypeId(Guid.NewGuid()),
            "Clinical Evaluation Report");

    public static void AddInitial(ProductDocumentAggregate document) =>
        document.AddInitialVersion(
            originalFileName: "cer.pdf",
            storedFileName: "stored-cer-v1.pdf",
            contentType: "application/pdf",
            fileSize: 1024,
            storagePath: "products/x/cer-v1.pdf",
            checksum: "sha256-v1");

    public static void AddNext(ProductDocumentAggregate document) =>
        document.AddNewVersion(
            originalFileName: "cer.pdf",
            storedFileName: "stored-cer-v2.pdf",
            contentType: "application/pdf",
            fileSize: 2048,
            storagePath: "products/x/cer-v2.pdf",
            checksum: "sha256-v2");
}
