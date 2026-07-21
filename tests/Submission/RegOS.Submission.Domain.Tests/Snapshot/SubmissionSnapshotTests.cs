using RegOS.SharedKernel.Primitives;
using FluentAssertions;

using RegOS.ProductDocument.Domain.IDs;
using RegOS.Submission.Domain.Snapshot;
using RegOS.Submission.Domain.Submission;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Submission.Domain.Tests.Snapshot;

public class SubmissionSnapshotTests
{
    private static (DocumentVersionId, int) Doc(int order) =>
        (DocumentVersionId.New(), order);

    [Fact]
    public void Create_AssignsSubmissionAndSnapshotId()
    {
        var submissionId = SubmissionId.New();

        var snapshot = SubmissionSnapshot.Create(TenantId.New(), submissionId, new[] { Doc(1) });

        snapshot.SubmissionId.Should().Be(submissionId);
        snapshot.Id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_PreservesVersionsAndDisplayOrderExactly()
    {
        var v1 = DocumentVersionId.New();
        var v2 = DocumentVersionId.New();
        var v3 = DocumentVersionId.New();

        // Non-contiguous display order (as a submission with a removed document
        // could have) is copied verbatim, not re-sequenced.
        var snapshot = SubmissionSnapshot.Create(TenantId.New(), 
            SubmissionId.New(),
            new[] { (v1, 1), (v2, 3), (v3, 4) });

        snapshot.Documents.Select(d => d.DocumentVersionId)
            .Should().ContainInOrder(v1, v2, v3);
        snapshot.Documents.Select(d => d.DisplayOrder)
            .Should().ContainInOrder(1, 3, 4);
    }

    [Fact]
    public void Create_WithNoDocuments_HasEmptyManifest()
    {
        var snapshot = SubmissionSnapshot.Create(TenantId.New(), 
            SubmissionId.New(),
            Array.Empty<(DocumentVersionId, int)>());

        snapshot.Documents.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithoutSubmission_Throws()
    {
        var act = () => SubmissionSnapshot.Create(TenantId.New(), default, new[] { Doc(1) });

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithEmptyVersion_Throws()
    {
        var act = () => SubmissionSnapshot.Create(TenantId.New(), 
            SubmissionId.New(),
            new[] { (default(DocumentVersionId), 1) });

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithDuplicateDisplayOrder_Throws()
    {
        var act = () => SubmissionSnapshot.Create(TenantId.New(), 
            SubmissionId.New(),
            new[] { (DocumentVersionId.New(), 1), (DocumentVersionId.New(), 1) });

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositiveDisplayOrder_Throws(int order)
    {
        var act = () => SubmissionSnapshot.Create(TenantId.New(), 
            SubmissionId.New(),
            new[] { (DocumentVersionId.New(), order) });

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Documents_IsReadOnly()
    {
        var snapshot = SubmissionSnapshot.Create(TenantId.New(), 
            SubmissionId.New(), new[] { Doc(1) });

        snapshot.Documents.Should().BeAssignableTo<IReadOnlyCollection<SnapshotDocument>>();
        snapshot.Documents.Should().ContainSingle();
    }
}
