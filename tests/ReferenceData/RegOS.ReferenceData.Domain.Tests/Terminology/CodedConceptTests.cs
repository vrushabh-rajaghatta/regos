using FluentAssertions;

using RegOS.ReferenceData.Domain.Terminology;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.ReferenceData.Domain.Tests.Terminology;

public class CodedConceptTests
{
    [Fact]
    public void Internal_StampsTheRegosSystem()
    {
        var concept = CodedConcept.Internal("CHEMICAL", "Chemical");

        concept.System.Should().Be("regos-internal");
        concept.IsInternal.Should().BeTrue();
    }

    /// <summary>
    /// The seam ADR-058 §3 is written around: a licensed code and a seeded one
    /// occupy the same field and are told apart by their system, which is what
    /// makes replacing the vocabulary a data migration.
    /// </summary>
    [Fact]
    public void ALicensedConceptIsNotInternal()
    {
        var concept = CodedConcept.Create("edqm", "10219000", "Tablet");

        concept.IsInternal.Should().BeFalse();
        concept.System.Should().Be("edqm");
    }

    [Fact]
    public void TwoConceptsWithTheSameSystemAndCodeAreEqual()
    {
        CodedConcept.Internal("CHEMICAL", "Chemical")
            .Should().Be(CodedConcept.Internal("CHEMICAL", "Chemical"));
    }

    /// <summary>
    /// A vocabulary's wording can be corrected without every value that quotes
    /// it ceasing to match.
    /// </summary>
    [Fact]
    public void ARewordedDisplayIsStillTheSameConcept()
    {
        var before = CodedConcept.Internal("CHEMICAL", "Chemical");
        var after = CodedConcept.Internal("CHEMICAL", "Chemical substance");

        after.Should().Be(before);
    }

    [Fact]
    public void TheSameCodeInADifferentSystemIsADifferentConcept()
    {
        var ours = CodedConcept.Internal("TAB", "Tablet");
        var theirs = CodedConcept.Create("edqm", "TAB", "Tablet");

        theirs.Should().NotBe(ours);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ASystemIsRequired(string? system)
    {
        var act = () => CodedConcept.Create(system!, "CHEMICAL", "Chemical");

        act.Should().Throw<DomainException>()
            .WithMessage(CodedConceptErrors.SystemRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ACodeIsRequired(string? code)
    {
        var act = () => CodedConcept.Internal(code!, "Chemical");

        act.Should().Throw<DomainException>()
            .WithMessage(CodedConceptErrors.CodeRequired);
    }

    [Fact]
    public void ADisplayIsRequired()
    {
        var act = () => CodedConcept.Internal("CHEMICAL", "  ");

        act.Should().Throw<DomainException>()
            .WithMessage(CodedConceptErrors.DisplayRequired);
    }

    [Fact]
    public void ValuesAreTrimmed()
    {
        var concept = CodedConcept.Internal(" CHEMICAL ", " Chemical ");

        concept.Code.Should().Be("CHEMICAL");
        concept.Display.Should().Be("Chemical");
    }
}
