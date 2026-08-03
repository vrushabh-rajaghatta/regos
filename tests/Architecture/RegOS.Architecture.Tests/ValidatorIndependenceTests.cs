using System.Text.RegularExpressions;

using FluentAssertions;

namespace RegOS.Architecture.Tests;

/// <summary>
/// <b>The validator is an oracle, not a dependency.</b> It exists to challenge
/// RegOS's reading of a specification — never to define it.
/// </summary>
/// <remarks>
/// <b>EPIC-007a's Definition of Done asked for exactly this, in exactly this
/// form:</b> *"the generator's only output is a complete sequence folder on disk
/// that an external tool is pointed at, and no code in `src/` reads a verdict
/// from any validator."* The seam is the filesystem, which every validator that
/// will ever exist already takes.
/// <para>
/// <b>The failure mode it forbids is building whatever the parser accepts.</b>
/// That would replace a public specification with one tool's reading of it, and
/// would make the oracle useless as evidence by making it no longer independent
/// of us. The epic's three most valuable findings came from a parser
/// disagreeing with RegOS; a parser RegOS consults cannot disagree with it.
/// </para>
/// <para>
/// <b>No <c>IEctdValidator</c> abstraction exists either</b>, and its absence is
/// deliberate rather than pending — ADR-018 forbids an interface for a single
/// implementation, and there is not even one implementation in <c>src/</c>.
/// </para>
/// </remarks>
public class ValidatorIndependenceTests
{
    /// <summary>
    /// Names of the tools that can return a verdict, and the shapes in which a
    /// verdict arrives.
    /// </summary>
    /// <remarks>
    /// <c>xmllint</c> and <c>xsltproc</c> are libxml2's; <c>eValidator</c> is
    /// LORENZ's, which Phase 1 could not obtain and EPIC-007b may. Matching the
    /// name rather than a process API is the point: it fails on the day someone
    /// shells out to one, whatever mechanism they choose.
    /// </remarks>
    private static readonly Regex ReadsAVerdict = new(
        @"\b(xmllint|xsltproc|eValidator|EctdValidator|IEctdValidator)\b",
        RegexOptions.Compiled);

    [Fact]
    public void NoCodeInSrc_ReadsAVerdictFromAValidator()
    {
        var offenders = Repo.SourceFiles("src")
            .Where(path => ReadsAVerdict.IsMatch(Repo.CodeOf(path)))
            .Select(Repo.Relative)
            .ToList();

        offenders.Should().BeEmpty(
            "the validator is an oracle, not a dependency (EPIC-007a) — the "
            + "seam between RegOS and any validator is the filesystem, and a "
            + "package is generated whether or not one is installed");
    }

    /// <summary>
    /// <b>The negative control.</b> A test that scans nothing passes, and would
    /// keep passing if <c>src/</c> moved or <c>Repo.Root</c> resolved somewhere
    /// unexpected — so the pass above means something only if the scan is real.
    /// </summary>
    [Fact]
    public void TheSourceTree_IsActuallyBeingRead()
    {
        Repo.SourceFiles("src").Should().HaveCountGreaterThan(100);
    }

    /// <summary>
    /// <b>And the harness is where it belongs.</b> <c>xmllint</c> lives in
    /// <c>tests/</c> because that is what it is — asserting its presence there
    /// stops the rule above from being satisfied by deleting the oracle instead
    /// of by keeping it outside the implementation.
    /// </summary>
    [Fact]
    public void TheOracle_IsStillInvokedFromTheTests()
    {
        var harnesses = Repo.SourceFiles("tests")
            .Where(path => ReadsAVerdict.IsMatch(Repo.CodeOf(path)))
            .ToList();

        harnesses.Should().NotBeEmpty(
            "a rule that no production code validates is trivially satisfied "
            + "by nothing validating at all");
    }
}
