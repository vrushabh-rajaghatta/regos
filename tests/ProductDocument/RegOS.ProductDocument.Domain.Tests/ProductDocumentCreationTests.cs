using RegOS.SharedKernel.Primitives;
using FluentAssertions;

using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.Enums;
using RegOS.ReferenceData.Domain.DocumentType;

using ProductDocumentAggregate =
    RegOS.ProductDocument.Domain.Aggregates.ProductDocument;

namespace RegOS.ProductDocument.Domain.Tests;

public class ProductDocumentCreationTests
{
    [Fact]
    public void Create_StartsInDraft()
    {
        TestFactory.NewDocument().Status
            .Should().Be(ProductDocumentStatus.Draft);
    }

    [Fact]
    public void Create_HasNoCurrentVersion()
    {
        TestFactory.NewDocument().CurrentVersionId.Should().BeNull();
    }

    [Fact]
    public void Create_HasNoVersions()
    {
        TestFactory.NewDocument().Versions.Should().BeEmpty();
    }

    [Fact]
    public void Create_SetsProvidedValues()
    {
        var productId = new ProductId(Guid.NewGuid());
        var documentTypeId = new DocumentTypeId(Guid.NewGuid());

        var document = ProductDocumentAggregate.Create(TenantId.New(), 
            productId,
            documentTypeId,
            "  Risk Management File  ");

        document.ProductId.Should().Be(productId);
        document.DocumentTypeId.Should().Be(documentTypeId);
        document.Name.Should().Be("Risk Management File");
        document.CreatedOnUtc.Should().NotBe(default);
    }
}
