using System.Diagnostics;
using System.Text;

using FluentAssertions;

using RegOS.Submission.Application.Generation;

namespace RegOS.Submission.Application.Tests;

/// <summary>
/// <b>EPIC-007a S005 — the ICH backbone, checked by something that did not
/// write it.</b>
///
/// Every assertion about validity here is made by <c>xmllint</c> against the
/// DTD <b>the package itself ships</b>, read out of the application assembly
/// rather than off disk. That is the epic's Level 2a discipline applied to
/// generated output: a normative machine-readable artifact plus a third-party
/// parser, neither of which is ours.
/// </summary>
/// <remarks>
/// <b>The validator is an oracle, not a dependency.</b> It lives in
/// <c>tests/</c> and no code in <c>src/</c> asks it anything — a package is
/// legal or not whether or not we ran a parser over it.
/// </remarks>
public sealed class IchBackboneRendererTests : IDisposable
{
    private readonly List<string> _roots = [];

    // --- The acceptance criterion --------------------------------------------

    /// <summary>
    /// The story's headline: a backbone built from frozen values alone, with
    /// leaves spread across four modules and both skipped levels exercised, is
    /// accepted by the DTD.
    /// </summary>
    [Fact]
    public void TheRenderedBackbone_IsValidAgainstTheDtdThePackageShips()
    {
        var xml = IchBackboneRenderer.Render(ADossierAcrossFourModules());

        var (exitCode, output) = Validate(xml);

        exitCode.Should().Be(0, "xmllint said: {0}", output);
    }

    /// <summary>
    /// <b>The negative controls, and they must bite.</b> A validator that
    /// accepts everything proves nothing, so the two failures the DTD is
    /// supposed to catch are provoked deliberately.
    /// </summary>
    /// <remarks>
    /// Both are hand-written, because the renderer cannot produce either — which
    /// is the point of writing them here rather than trusting that it cannot.
    /// </remarks>
    [Theory]
    [InlineData("carried-forward", "checksum=\"abc\"", "operation")]
    [InlineData("new", "", "checksum")]
    public void TheDtd_RejectsWhatItIsSupposedToReject(
        string operation, string checksum, string expectedComplaint)
    {
        var xml = $"""
            <?xml version="1.0" encoding="UTF-8" standalone="no"?>
            <!DOCTYPE ectd:ectd SYSTEM "{IchBackboneRenderer.DoctypeSystemId}">
            <ectd:ectd xmlns:ectd="http://www.ich.org/ectd"
                       xmlns:xlink="http://www.w3c.org/1999/xlink"
                       dtd-version="3.2">
              <m2-common-technical-document-summaries>
                <leaf ID="leaf-1" operation="{operation}" {checksum}
                      checksum-type="md5" xlink:href="m2/summary.pdf">
                  <title>Summary</title>
                </leaf>
              </m2-common-technical-document-summaries>
            </ectd:ectd>
            """;

        var (exitCode, output) = Validate(xml);

        exitCode.Should().NotBe(0,
            "a bad {0} has to fail, or the oracle is not reading",
            expectedComplaint);

        output.Should().Contain(expectedComplaint);
    }

    /// <summary>
    /// <b>Why the third refusal exists, proved rather than asserted.</b> Four
    /// backbone elements are repeatable nodes keyed by a business fact —
    /// <c>substance</c>, <c>manufacturer</c>, <c>indication</c> — and RegOS's
    /// blueprint models each as a single section. Writing one without its key is
    /// not untidy; the DTD rejects it.
    /// </summary>
    /// <remarks>
    /// This test is the evidence behind
    /// <see cref="SequenceGenerationErrors.SectionNeedsAFactRegOsDoesNotHold"/>.
    /// Without it, that refusal is a claim about a specification rather than a
    /// demonstrated fact about one.
    /// </remarks>
    [Theory]
    [InlineData("m3-quality/m3-2-body-of-data/m3-2-s-drug-substance", "substance")]
    [InlineData("m5-clinical-study-reports/m5-3-clinical-study-reports/"
        + "m5-3-5-reports-of-efficacy-and-safety-studies", "indication")]
    public void AKeyedElement_IsRejectedWithoutTheFactThatIdentifiesIt(
        string chain, string missingAttribute)
    {
        var path = chain.Split('/');

        var xml = IchBackboneRenderer.Render([
            Leaf(path, "keyed", "somewhere/report.pdf"),
        ]);

        var (exitCode, output) = Validate(xml);

        exitCode.Should().NotBe(0);
        output.Should().Contain($"does not carry attribute {missingAttribute}");

        // And the renderer names it, so the generator can refuse before writing.
        IchBackboneRenderer.KeyedElements.Should().ContainKey(path[^1]);
    }

    // --- What the tree has to do ---------------------------------------------

    /// <summary>
    /// <b>Shared prefixes merge.</b> Three sections beneath 4.2 name
    /// <c>m4-2-study-reports</c> in their chains, and the backbone must open
    /// that element once — not three times, which the DTD would reject outright
    /// since <c>m4-nonclinical-study-reports</c> declares it <c>?</c>.
    /// </summary>
    [Fact]
    public void SectionsSharingAnAncestor_EmitItOnce()
    {
        var xml = IchBackboneRenderer.Render([
            Leaf(["m4-nonclinical-study-reports", "m4-2-study-reports",
                  "m4-2-1-pharmacology"], "a", "m4/42/421/primary.pdf"),
            Leaf(["m4-nonclinical-study-reports", "m4-2-study-reports",
                  "m4-2-2-pharmacokinetics"], "b", "m4/42/422/pk.pdf"),
            Leaf(["m4-nonclinical-study-reports", "m4-2-study-reports",
                  "m4-2-3-toxicology"], "c", "m4/42/423/tox.pdf"),
        ]);

        Occurrences(xml, "<m4-2-study-reports>").Should().Be(1);
        Occurrences(xml, "<m4-nonclinical-study-reports>").Should().Be(1);

        Validate(xml).ExitCode.Should().Be(0);
    }

    /// <summary>
    /// Every container is declared <c>(leaf*, …)</c> — an ordered sequence. A
    /// leaf written after a child element is invalid however sensible it reads,
    /// and this is the case where a naive tree walk gets it wrong.
    /// </summary>
    [Fact]
    public void ALeafOnAContainer_IsWrittenBeforeThatContainersChildren()
    {
        var xml = IchBackboneRenderer.Render([
            Leaf(["m3-quality", "m3-2-body-of-data", "m3-2-p-drug-product"],
                "child", "m3/32/32s/substance.pdf"),
            Leaf(["m3-quality"], "onTheModule", "m3/overview.pdf"),
        ]);

        xml.IndexOf("overview.pdf", StringComparison.Ordinal).Should()
            .BeLessThan(xml.IndexOf("<m3-2-body-of-data>", StringComparison.Ordinal));

        Validate(xml).ExitCode.Should().Be(0);
    }

    /// <summary>
    /// ADR-049 again, one level down from S004: the projection thesis has to
    /// hold for the XML as well as for the files it describes.
    /// </summary>
    [Fact]
    public void RenderingTwice_ProducesTheSameText()
    {
        var leaves = ADossierAcrossFourModules();

        IchBackboneRenderer.Render(leaves)
            .Should().Be(IchBackboneRenderer.Render(leaves));
    }

    /// <summary>
    /// Leaf order cannot depend on the order the caller happened to supply, or
    /// two runs of the same frozen submission could differ.
    /// </summary>
    [Fact]
    public void TheSameLeavesInADifferentOrder_RenderIdentically()
    {
        var leaves = ADossierAcrossFourModules();
        var shuffled = leaves.AsEnumerable().Reverse().ToList();

        IchBackboneRenderer.Render(shuffled)
            .Should().Be(IchBackboneRenderer.Render(leaves));
    }

    // --- The operations ------------------------------------------------------

    /// <summary>
    /// <c>modified-file</c> carries the sequence folder and the target leaf's ID
    /// (ICH Appendix 6), because a leaf ID is unique only within its own
    /// sequence and is never reused across them.
    /// </summary>
    [Fact]
    public void AReplacement_PointsAtTheSequenceAndLeafItSupersedes()
    {
        var xml = IchBackboneRenderer.Render([
            Leaf(["m2-common-technical-document-summaries"], "now",
                "m2/summary.pdf") with
            {
                Operation = "replace",
                ModifiedFile = "../0000/index.xml#leaf-then",
            },
        ]);

        xml.Should().Contain("operation=\"replace\"");
        xml.Should().Contain("modified-file=\"../0000/index.xml#leaf-then\"");

        Validate(xml).ExitCode.Should().Be(0);
    }

    /// <summary>
    /// ICH Appendix 6 Table 6-3: <i>"there is no new file submitted in this
    /// case… the checksum attribute value will be empty i.e., double quotation
    /// marks with no entry between."</i> The empty string is the specification's
    /// instruction, not an omission — and <c>checksum</c> is <c>#REQUIRED</c>,
    /// so leaving it out would be invalid.
    /// </summary>
    [Fact]
    public void AWithdrawal_CarriesAnEmptyChecksumAndAnEmptyHref()
    {
        var xml = IchBackboneRenderer.Render([
            new BackboneLeaf(
                ["m2-common-technical-document-summaries"],
                "leaf-gone", "Withdrawn summary",
                Href: string.Empty,
                Operation: "delete",
                Checksum: string.Empty,
                ModifiedFile: "../0000/index.xml#leaf-was"),
        ]);

        xml.Should().Contain("checksum=\"\"");
        xml.Should().Contain("xlink:href=\"\"");

        Validate(xml).ExitCode.Should().Be(0);
    }

    // --- The story boundary --------------------------------------------------

    /// <summary>
    /// <b>Deliberately ignorant of FDA.</b> None of S003's vocabulary appears in
    /// this file, because none of it appears in the ICH DTD. A renderer that had
    /// reached for a wire token would show it here.
    /// </summary>
    [Fact]
    public void TheIchBackbone_NamesNoAuthoritysVocabulary()
    {
        var xml = IchBackboneRenderer.Render(ADossierAcrossFourModules());

        xml.Should().NotContain("submission-type");
        xml.Should().NotContain("submission-sub-type");
        xml.Should().NotContain("application-type");
        xml.Should().NotContain("fda");
    }

    /// <summary>
    /// The DOCTYPE has to name the file <see cref="SequenceFolderGenerator"/>
    /// writes into <c>util/dtd/</c>. If these drift apart the package validates
    /// against a DTD it does not carry, which is the failure a reviewer cannot
    /// see and a regulator can.
    /// </summary>
    [Fact]
    public void TheDoctype_NamesTheDtdTheGeneratorWrites()
    {
        IchBackboneRenderer.DoctypeSystemId.Should().Be("util/dtd/ich-ectd-3-2.dtd");

        IchBackboneRenderer.Render([]).Should().Contain(
            $"<!DOCTYPE ectd:ectd SYSTEM \"{IchBackboneRenderer.DoctypeSystemId}\">");
    }

    // --- Fixtures ------------------------------------------------------------

    /// <summary>
    /// A dossier wide enough to be worth validating: four modules, both levels
    /// the blueprint chains rather than models, and two leaves sharing a parent.
    /// </summary>
    private static List<BackboneLeaf> ADossierAcrossFourModules() =>
    [
        Leaf(["m2-common-technical-document-summaries",
              "m2-3-quality-overall-summary"],
            "qos", "m2/23-quality-overall-summary/qos.pdf"),

        // The first skipped level: RegOS has no 3.2 node, so the value chains.
        Leaf(["m3-quality", "m3-2-body-of-data", "m3-2-p-drug-product"],
            "substance", "m3/32-body-data/32s-drug-sub/spec.pdf"),
        Leaf(["m3-quality", "m3-2-body-of-data", "m3-2-p-drug-product"],
            "substance2", "m3/32-body-data/32s-drug-sub/stability.pdf"),

        // The second: no 4.2 node either.
        Leaf(["m4-nonclinical-study-reports", "m4-2-study-reports",
              "m4-2-1-pharmacology"],
            "pharm", "m4/42-study-rep/421-pharmacol/primary.pdf"),

        Leaf(["m5-clinical-study-reports", "m5-3-clinical-study-reports",
              "m5-3-1-reports-of-biopharmaceutic-studies"],
            "biopharm", "m5/53-clin-stud-rep/531-biopharm-stud-rep/ba.pdf"),
    ];

    private static BackboneLeaf Leaf(
        string[] elementPath, string id, string href) =>
        new(elementPath, $"leaf-{id}", $"Document {id}", href, "new",
            "0123456789abcdef0123456789abcdef");

    private static int Occurrences(string text, string value) =>
        text.Split(value).Length - 1;

    /// <summary>
    /// Writes the backbone into a throwaway package laid out the way a real one
    /// is — <c>index.xml</c> at the root, the DTD under <c>util/dtd/</c> — and
    /// asks xmllint. The relative DOCTYPE is resolved from that layout, so this
    /// checks the path as well as the content.
    /// </summary>
    private (int ExitCode, string Output) Validate(string xml)
    {
        var root = Path.Combine(
            Path.GetTempPath(), "regos-s005", Guid.NewGuid().ToString("N"));

        _roots.Add(root);
        Directory.CreateDirectory(Path.Combine(root, "util", "dtd"));

        // The DTD the package ships, out of the assembly that ships it.
        using (var resource = typeof(SequenceFolderGenerator).Assembly
            .GetManifestResourceStream(
                "RegOS.Submission.Application.Generation.ich-ectd-3-2.dtd")
            ?? throw new InvalidOperationException(
                "The ICH DTD is not embedded in this build."))
        using (var file = File.Create(
            Path.Combine(root, "util", "dtd", "ich-ectd-3-2.dtd")))
        {
            resource.CopyTo(file);
        }

        var indexPath = Path.Combine(root, IchBackboneRenderer.FileName);
        File.WriteAllText(indexPath, xml, new UTF8Encoding(false));

        // Fully qualified since EPIC-020 S001: RegOS.Process is a bounded context,
        // and inside the RegOS root namespace it now shadows System.Diagnostics.
        using var process = System.Diagnostics.Process.Start(
            new ProcessStartInfo("xmllint")
        {
            ArgumentList = { "--noout", "--valid", indexPath },
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        })!;

        var output = process.StandardError.ReadToEnd()
            + process.StandardOutput.ReadToEnd();

        process.WaitForExit();

        return (process.ExitCode, output);
    }

    public void Dispose()
    {
        foreach (var root in _roots.Where(Directory.Exists))
            Directory.Delete(root, recursive: true);
    }
}
