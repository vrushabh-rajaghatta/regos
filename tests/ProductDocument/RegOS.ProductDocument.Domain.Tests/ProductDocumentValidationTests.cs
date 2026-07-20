using FluentAssertions;

using RegOS.Product.Domain.Product;
using RegOS.ProductDocument.Domain.Errors;
using RegOS.ReferenceData.Domain.DocumentType;
using RegOS.SharedKernel.Exceptions;

using ProductDocumentAggregate =
    RegOS.ProductDocument.Domain.Aggregates.ProductDocument;

namespace RegOS.ProductDocument.Domain.Tests;

public class ProductDocumentValidationTests
{
    private static ProductDocumentAggregate Create(
        ProductId productId,
        DocumentTypeId documentTypeId,
        string name)
        => ProductDocumentAggregate.Create(productId, documentTypeId, name);

    [Fact]
    public void Create_WithDefaultProductId_Throws()
    {
        var act = () => Create(
            default,
            new DocumentTypeId(Guid.NewGuid()),
            "Label");

        act.Should().Throw<DomainException>()
            .WithMessage($"{ProductDocumentErrors.ProductRequired}*");
    }

    [Fact]
    public void Create_WithDefaultDocumentTypeId_Throws()
    {
        var act = () => Create(
            new ProductId(Guid.NewGuid()),
            default,
            "Label");

        act.Should().Throw<DomainException>()
            .WithMessage($"{ProductDocumentErrors.DocumentTypeRequired}*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithBlankName_Throws(string? name)
    {
        var act = () => Create(
            new ProductId(Guid.NewGuid()),
            new DocumentTypeId(Guid.NewGuid()),
            name!);

        act.Should().Throw<DomainException>()
            .WithMessage($"{ProductDocumentErrors.DocumentNameRequired}*");
    }

    [Fact]
    public void Create_WithNameOverMaxLength_Throws()
    {
        var name = new string('a', ProductDocumentAggregate.NameMaxLength + 1);

        var act = () => Create(
            new ProductId(Guid.NewGuid()),
            new DocumentTypeId(Guid.NewGuid()),
            name);

        act.Should().Throw<DomainException>()
            .WithMessage($"{ProductDocumentErrors.DocumentNameTooLong}*");
    }

    [Fact]
    public void AddInitialVersion_WithBlankOriginalFileName_Throws()
    {
        var document = TestFactory.NewDocument();

        var act = () => document.AddInitialVersion(
            " ", "stored.pdf", "application/pdf", 10, "path", "sum");

        act.Should().Throw<DomainException>()
            .WithMessage($"{ProductDocumentErrors.OriginalFileNameRequired}*");
    }

    [Fact]
    public void AddInitialVersion_WithBlankStoredFileName_Throws()
    {
        var document = TestFactory.NewDocument();

        var act = () => document.AddInitialVersion(
            "original.pdf", " ", "application/pdf", 10, "path", "sum");

        act.Should().Throw<DomainException>()
            .WithMessage($"{ProductDocumentErrors.StoredFileNameRequired}*");
    }

    [Fact]
    public void AddInitialVersion_WithBlankContentType_Throws()
    {
        var document = TestFactory.NewDocument();

        var act = () => document.AddInitialVersion(
            "original.pdf", "stored.pdf", " ", 10, "path", "sum");

        act.Should().Throw<DomainException>()
            .WithMessage($"{ProductDocumentErrors.ContentTypeRequired}*");
    }

    [Fact]
    public void AddInitialVersion_WithBlankStoragePath_Throws()
    {
        var document = TestFactory.NewDocument();

        var act = () => document.AddInitialVersion(
            "original.pdf", "stored.pdf", "application/pdf", 10, " ", "sum");

        act.Should().Throw<DomainException>()
            .WithMessage($"{ProductDocumentErrors.InvalidStoragePath}*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddInitialVersion_WithNonPositiveFileSize_Throws(long fileSize)
    {
        var document = TestFactory.NewDocument();

        var act = () => document.AddInitialVersion(
            "original.pdf", "stored.pdf", "application/pdf", fileSize, "path", "sum");

        act.Should().Throw<DomainException>()
            .WithMessage($"{ProductDocumentErrors.InvalidFileSize}*");
    }
}
