using System.Linq;

using FluentAssertions;

using RegOS.ProductDocument.Domain.Errors;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.ProductDocument.Domain.Tests;

public class ProductDocumentVersioningTests
{
    [Fact]
    public void AddInitialVersion_NumbersItOne()
    {
        var document = TestFactory.NewDocument();

        TestFactory.AddInitial(document);

        document.Versions.Should().ContainSingle();
        document.Versions.Single().VersionNumber.Should().Be(1);
    }

    [Fact]
    public void AddInitialVersion_SetsCurrentVersion()
    {
        var document = TestFactory.NewDocument();

        TestFactory.AddInitial(document);

        document.CurrentVersionId
            .Should().Be(document.Versions.Single().Id);
    }

    [Fact]
    public void AddInitialVersion_CapturesFileMetadata()
    {
        var document = TestFactory.NewDocument();

        TestFactory.AddInitial(document);

        var version = document.Versions.Single();
        version.OriginalFileName.Should().Be("cer.pdf");
        version.StoredFileName.Should().Be("stored-cer-v1.pdf");
        version.ContentType.Should().Be("application/pdf");
        version.FileSize.Should().Be(1024);
        version.StoragePath.Should().Be("products/x/cer-v1.pdf");
        version.Checksum.Should().Be("sha256-v1");
        version.UploadedOnUtc.Should().NotBe(default);
    }

    [Fact]
    public void AddInitialVersion_CalledTwice_Throws()
    {
        var document = TestFactory.NewDocument();
        TestFactory.AddInitial(document);

        var act = () => TestFactory.AddInitial(document);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ProductDocumentErrors.DocumentAlreadyHasInitialVersion);
    }

    [Fact]
    public void AddNewVersion_IncrementsVersionNumber()
    {
        var document = TestFactory.NewDocument();
        TestFactory.AddInitial(document);

        TestFactory.AddNext(document);

        document.Versions.Should().HaveCount(2);
        document.Versions.Max(v => v.VersionNumber).Should().Be(2);
    }

    [Fact]
    public void AddNewVersion_UpdatesCurrentVersion()
    {
        var document = TestFactory.NewDocument();
        TestFactory.AddInitial(document);
        var initialVersionId = document.CurrentVersionId;

        TestFactory.AddNext(document);

        document.CurrentVersionId.Should().NotBe(initialVersionId);
        var latest = document.Versions.Single(v => v.VersionNumber == 2);
        document.CurrentVersionId.Should().Be(latest.Id);
    }

    [Fact]
    public void AddNewVersion_RetainsPreviousVersion()
    {
        var document = TestFactory.NewDocument();
        TestFactory.AddInitial(document);

        TestFactory.AddNext(document);

        document.Versions.Should().Contain(v => v.VersionNumber == 1);
        document.Versions.Should().Contain(v => v.VersionNumber == 2);
    }

    [Fact]
    public void AddNewVersion_ProducesSequentialNumbering()
    {
        var document = TestFactory.NewDocument();
        TestFactory.AddInitial(document);
        TestFactory.AddNext(document);

        TestFactory.AddNext(document);

        document.Versions.Select(v => v.VersionNumber).OrderBy(n => n)
            .Should().Equal(1, 2, 3);
    }

    [Fact]
    public void AddNewVersion_WithoutInitialVersion_Throws()
    {
        var document = TestFactory.NewDocument();

        var act = () => TestFactory.AddNext(document);

        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage(ProductDocumentErrors.DocumentHasNoInitialVersion);
    }
}
