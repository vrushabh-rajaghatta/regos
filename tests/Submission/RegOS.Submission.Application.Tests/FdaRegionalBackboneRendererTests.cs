using System.Diagnostics;
using System.Text;

using FluentAssertions;

using RegOS.Submission.Application.Generation;

namespace RegOS.Submission.Application.Tests;

/// <summary>
/// <b>EPIC-007a S006 — the regional backbone, checked by FDA's own DTD.</b>
///
/// The file is laid out where a real package puts it — <c>m1/us/</c>, with the
/// DTD two levels up under <c>util/dtd/</c> — so the relative DOCTYPE is
/// resolved the way a regulator's tool would resolve it, and the path is checked
/// along with the content.
/// </summary>
public sealed class FdaRegionalBackboneRendererTests : IDisposable
{
    private readonly List<string> _roots = [];

    // --- The acceptance criterion --------------------------------------------

    [Fact]
    public void TheRegionalBackbone_IsValidAgainstTheDtdThePackageShips()
    {
        var xml = FdaRegionalBackboneRenderer.Render(AnIndSequence());

        var (exitCode, output) = Validate(xml);

        exitCode.Should().Be(0, "xmllint said: {0}", output);
    }

    /// <summary>
    /// The negative control. <c>admin</c> is not optional, and the epic's
    /// existing evidence says this exact omission is what the DTD catches.
    /// </summary>
    [Fact]
    public void TheDtd_RejectsAnEnvelopeWithNoContacts()
    {
        var xml = $"""
            <?xml version="1.0" encoding="UTF-8" standalone="no"?>
            <!DOCTYPE fda-regional:fda-regional SYSTEM "{FdaRegionalBackboneRenderer.DoctypeSystemId}">
            <fda-regional:fda-regional xmlns:fda-regional="http://www.ich.org/fda"
                xmlns:xlink="http://www.w3c.org/1999/xlink" dtd-version="3.3">
              <admin>
                <applicant-info>
                  <id>999999999</id>
                  <company-name>Demo</company-name>
                </applicant-info>
                <application-set>
                  <application application-containing-files="true">
                    <application-information>
                      <application-number application-type="fdaat4">1</application-number>
                    </application-information>
                    <submission-information>
                      <submission-id submission-type="fdast1">0000</submission-id>
                      <sequence-number submission-sub-type="fdasst1">0000</sequence-number>
                    </submission-information>
                  </application>
                </application-set>
              </admin>
            </fda-regional:fda-regional>
            """;

        var (exitCode, output) = Validate(xml);

        exitCode.Should().NotBe(0);
        output.Should().Contain("applicant-contacts");
    }

    // --- What the format demands ---------------------------------------------

    /// <summary>
    /// <c>m1-regional</c>'s content model is a sequence, and the blueprint seeds
    /// 1.13 and 1.14 — which sort <em>before</em> 1.2 in every ordinal
    /// comparison there is. Emitting in the order sections happen to arrive is
    /// invalid, and this is the case that proves it.
    /// </summary>
    [Fact]
    public void ModuleOneSections_AreEmittedInTheOrderTheDtdDeclares()
    {
        // Supplied 1.14.4.1 first, so passing cannot be an accident of input
        // order. Ordinal sorting would also emit m1-14 before m1-2.
        var xml = FdaRegionalBackboneRenderer.Render(AnIndSequence() with
        {
            Leaves =
            [
                Leaf(["m1-14-labeling", "m1-14-4-investigational-drug-labeling",
                      "m1-14-4-1-investigational-brochure"],
                    "brochure", "m1/us/114-labeling/1144-inv/ib.pdf"),
                Leaf(["m1-2-cover-letters"], "cover", "m1/us/12-cover-letters/cl.pdf"),
            ],
        });

        var cover = xml.IndexOf("<m1-2-cover-letters>", StringComparison.Ordinal);
        var labeling = xml.IndexOf("<m1-14-labeling>", StringComparison.Ordinal);

        cover.Should().BeLessThan(labeling);

        // And the chain nests rather than flattening — 1.14.4.1 is three
        // elements deep, not a sibling of 1.2.
        xml.Should().Contain("<m1-14-4-investigational-drug-labeling>");
        xml.Should().Contain("<m1-14-4-1-investigational-brochure>");

        var (exitCode, output) = Validate(xml);
        exitCode.Should().Be(0, "xmllint said: {0}", output);
    }

    /// <summary>
    /// <b>E19, proved rather than asserted.</b> Of the eight Module 1 sections
    /// the FDA IND blueprint offers as placement targets, only two can hold a
    /// document. 1.14 Labeling is declared as child elements and no
    /// <c>leaf</c> — so the blueprint's tree and the backbone's tree disagree
    /// about which nodes bear content.
    /// </summary>
    [Fact]
    public void AContainerOnlySection_CannotHoldADocument()
    {
        var xml = FdaRegionalBackboneRenderer.Render(AnIndSequence() with
        {
            Leaves = [Leaf(["m1-14-labeling"], "label", "m1/us/114-labeling/l.pdf")],
        });

        var (exitCode, output) = Validate(xml);

        exitCode.Should().NotBe(0);
        output.Should().Contain("m1-14-labeling");

        FdaRegionalBackboneRenderer.ContainerOnlyElements
            .Should().Contain("m1-14-labeling");
    }

    /// <summary>
    /// <b>E18, proved rather than asserted.</b> <c>m1-1-forms</c> holds
    /// <c>form*</c>, not <c>leaf*</c>, and each <c>form</c> requires a
    /// <c>form-type</c> RegOS does not model. A leaf placed there is rejected —
    /// which is why generation refuses instead of guessing <c>fdaft1</c>.
    /// </summary>
    [Fact]
    public void AFormsLeaf_IsRejected_BecauseFormsHoldFormsNotLeaves()
    {
        var xml = FdaRegionalBackboneRenderer.Render(AnIndSequence() with
        {
            Leaves = [Leaf(["m1-1-forms"], "form", "m1/us/11-forms/1571.pdf")],
        });

        var (exitCode, output) = Validate(xml);

        exitCode.Should().NotBe(0);
        output.Should().Contain("m1-1-forms");

        FdaRegionalBackboneRenderer.KeyedElements.Should().ContainKey("m1-1-forms");
    }

    /// <summary>
    /// <c>fda-regional</c> is <c>(admin, m1-regional?)</c> — the envelope is
    /// mandatory and the module is not. A sequence whose content is all Modules
    /// 2–5 still files a regional file saying who filed it.
    /// </summary>
    [Fact]
    public void ASequenceWithNoModuleOneContent_StillFilesItsEnvelope()
    {
        var xml = FdaRegionalBackboneRenderer.Render(
            AnIndSequence() with { Leaves = [] });

        xml.Should().Contain("<admin>");
        xml.Should().NotContain("<m1-regional>");

        Validate(xml).ExitCode.Should().Be(0);
    }

    /// <summary>
    /// <b>E23 — a construct the format permits and the authority refuses.</b>
    /// ICH declares <c>node-extension</c> in most content models
    /// (<c>((leaf | node-extension)*)</c>), and FDA's eCTD Technical Conformance
    /// Guide §5 says it is *"not acceptable in any submissions to FDA"*.
    /// </summary>
    /// <remarks>
    /// Neither renderer emits one. <b>Today that is a property of the
    /// implementation; this makes it a property of the design</b> — the failure
    /// mode being someone adding support later on the entirely reasonable
    /// grounds that the DTD allows it. Asserted for both backbones, because the
    /// prohibition is regional and <c>index.xml</c> travels in the same package.
    /// </remarks>
    [Fact]
    public void NeitherBackbone_EmitsANodeExtension()
    {
        var regional = FdaRegionalBackboneRenderer.Render(AnIndSequence());

        var ich = IchBackboneRenderer.Render([
            new BackboneLeaf(
                ["m2-common-technical-document-summaries"],
                "leaf-x", "Summary", "m2/summary.pdf", "new", "abc"),
        ]);

        regional.Should().NotContain("node-extension");
        ich.Should().NotContain("node-extension");
    }

    // --- E16, carried forward -------------------------------------------------

    /// <summary>
    /// <b>The asymmetry, travelled in the safe direction.</b> Here
    /// <c>checksum</c> is <c>#IMPLIED</c>; in <c>index.xml</c> it is
    /// <c>#REQUIRED</c>. Writing it in both places means the habit this renderer
    /// teaches stays correct when carried to the stricter file — the reverse
    /// produces a package that passes file-by-file and fails as a whole.
    /// </summary>
    [Fact]
    public void ARegionalLeaf_CarriesItsChecksumEvenThoughFdaDoesNotRequireIt()
    {
        var xml = FdaRegionalBackboneRenderer.Render(AnIndSequence());

        xml.Should().Contain("checksum=\"");
        xml.Should().Contain("checksum-type=\"md5\"");

        Validate(xml).ExitCode.Should().Be(0);
    }

    /// <summary>S003 stops being a design and becomes three attributes.</summary>
    [Fact]
    public void TheRegulatoryActivity_IsNamedByTheSequenceThatOpenedIt()
    {
        var xml = FdaRegionalBackboneRenderer.Render(AnIndSequence() with
        {
            SubmissionId = "0000",
            SequenceNumber = "0003",
        });

        xml.Should().Contain("<submission-id submission-type=\"fdast1\">0000</submission-id>");
        xml.Should().Contain(
            "<sequence-number submission-sub-type=\"fdasst2\">0003</sequence-number>");
    }

    [Fact]
    public void RenderingTwice_ProducesTheSameText()
    {
        var backbone = AnIndSequence();

        FdaRegionalBackboneRenderer.Render(backbone)
            .Should().Be(FdaRegionalBackboneRenderer.Render(backbone));
    }

    /// <summary>
    /// <b>E26 — the header is FDA's, verbatim.</b> The Module 1 Backbone Files
    /// Specification §II calls it *"always the same"*: a DOCTYPE pointing at
    /// accessdata.fda.gov and a stylesheet processing instruction beside it.
    /// </summary>
    /// <remarks>
    /// This renderer emitted a <c>../../util/dtd/</c> path and no stylesheet at
    /// all, on the reasonable assumption that a regional backbone resolves its
    /// DTD the way the ICH one does. Appendix 2 §E.17 records that the util-folder
    /// form is what v2.0 <em>replaced</em>. Only the specification said so.
    /// </remarks>
    [Fact]
    public void TheHeader_IsTheOneTheSpecificationStates()
    {
        var xml = FdaRegionalBackboneRenderer.Render(AnIndSequence());

        xml.Should().Contain(
            "<!DOCTYPE fda-regional:fda-regional SYSTEM "
            + "\"https://www.accessdata.fda.gov/static/eCTD/us-regional-v3-3.dtd\">");

        xml.Should().Contain(
            "<?xml-stylesheet type=\"text/xsl\" "
            + "href=\"https://www.accessdata.fda.gov/static/eCTD/us-regional.xsl\"?>");

        // And nothing left over from what it used to emit.
        xml.Should().NotContain("util/dtd");
    }

    // --- Fixtures ------------------------------------------------------------

    /// <remarks>
    /// Every wire value here is a <b>test</b> value, supplied to the renderer
    /// rather than known by it. <c>123456789</c> is a fictional DUNS and
    /// <c>fdatnt1</c> a token whose vocabulary RegOS has not read — both are
    /// legitimate here, where the question is *"can this renderer express a valid
    /// file?"*, and neither may reach a generator until a specification we hold
    /// supplies it.
    /// </remarks>
    private static RegionalBackbone AnIndSequence() => new(
        ApplicantId: "123456789",
        CompanyName: "Demo Manufacturer Ltd.",
        SubmissionDescription: "Original IND",
        Contacts:
        [
            new RegionalContact(
                "Ana Ruiz", "fdaact1",
                [new RegionalTelephone("1-555-0100", "fdatnt1")],
                ["ana.ruiz@example.com"]),
        ],
        ApplicationNumber: "987654",
        ApplicationType: "fdaat4",
        SubmissionId: "0000",
        SubmissionType: "fdast1",
        SequenceNumber: "0003",
        SubmissionSubType: "fdasst2",
        Leaves: [Leaf(["m1-2-cover-letters"], "cover", "m1/us/12-cover-letters/cl.pdf")]);

    private static BackboneLeaf Leaf(
        IReadOnlyList<string> elementPath, string id, string href) =>
        new(elementPath, $"leaf-{id}", $"Document {id}", href, "new",
            "0123456789abcdef0123456789abcdef");

    private (int ExitCode, string Output) Validate(string xml)
    {
        var root = Path.Combine(
            Path.GetTempPath(), "regos-s006", Guid.NewGuid().ToString("N"));

        _roots.Add(root);

        var regional = Path.Combine(root, "m1", "us");
        Directory.CreateDirectory(regional);
        Directory.CreateDirectory(Path.Combine(root, "util", "dtd"));

        using (var resource = typeof(SequenceFolderGenerator).Assembly
            .GetManifestResourceStream(
                "RegOS.Submission.Application.Generation.us-regional-v3-3.dtd")
            ?? throw new InvalidOperationException(
                "The FDA regional DTD is not embedded in this build."))
        using (var file = File.Create(
            Path.Combine(root, "util", "dtd", "us-regional-v3-3.dtd")))
        {
            resource.CopyTo(file);
        }

        // FDA's header points the DOCTYPE at accessdata.fda.gov (E26), and the
        // epic's Level 2a claim rests on offline validation against a pinned DTD.
        // Rewriting it here keeps both honest: what ships carries FDA's URL — see
        // TheHeader_IsTheOneTheSpecificationStates — and what is validated is the
        // DTD this repository pins. Bending either to suit the other would be the
        // dishonest fix.
        var offline = xml.Replace(
            FdaRegionalBackboneRenderer.DoctypeSystemId,
            "../../util/dtd/us-regional-v3-3.dtd",
            StringComparison.Ordinal);

        var path = Path.Combine(regional, "us-regional.xml");
        File.WriteAllText(path, offline, new UTF8Encoding(false));

        // Fully qualified since EPIC-020 S001: RegOS.Process is a bounded context,
        // and inside the RegOS root namespace it now shadows System.Diagnostics.
        using var process = System.Diagnostics.Process.Start(
            new ProcessStartInfo("xmllint")
        {
            ArgumentList = { "--noout", "--valid", path },
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
