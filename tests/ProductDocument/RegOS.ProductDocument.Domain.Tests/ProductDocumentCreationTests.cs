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
        var globalProductId = new GlobalProductId(Guid.NewGuid());
        var documentTypeId = new DocumentTypeId(Guid.NewGuid());

        var document = ProductDocumentAggregate.Create(TenantId.New(), 
            globalProductId,
            documentTypeId,
            "  Risk Management File  ");

        document.GlobalProductId.Should().Be(globalProductId);
        document.DocumentTypeId.Should().Be(documentTypeId);
        document.Name.Should().Be("Risk Management File");
        document.CreatedOnUtc.Should().NotBe(default);
    }
}
