using System.Text;
using System.Xml;

namespace RegOS.Submission.Application.Generation;

/// <summary>
/// Renders <c>m1/us/us-regional.xml</c> — FDA's Module 1 backbone.
/// </summary>
/// <remarks>
/// <b>EPIC-007a S006, and deliberately not a subclass of anything.</b>
/// <see cref="IchBackboneRenderer"/> is not refactored into a shared base with
/// this one. Two renderers sit over one projection because
/// <see href="../../../docs/evidence/README.md">E16</see> showed the backbones
/// are separate contracts rather than one ruleset with regional flags — and
/// [ADR-018](../../../docs/adr/ADR-018-rule-of-three.md) forbids abstracting a
/// boundary on a single demonstrated divergence. This story is where that
/// temptation peaks; the shared code between the two files is a leaf element
/// with different attribute rules, which is exactly what must not be unified.
/// <para>
/// <b>This is where S003 stops being a design and becomes an attribute.</b>
/// <c>submission-id</c> is the sequence that opened the regulatory activity;
/// <c>submission-type</c> and <c>submission-sub-type</c> are its wire tokens.
/// None of them appears in <c>index.xml</c>, which is why S005 could be written
/// without them.
/// </para>
/// </remarks>
public static class FdaRegionalBackboneRenderer
{
    /// <summary>Where the regional file sits — the M1 section's own folder.</summary>
    public const string RelativePath = "m1/us/us-regional.xml";

    /// <summary>
    /// <b>A URL, not a path into <c>util/</c></b> — the Module 1 Backbone Files
    /// Specification §II states the header verbatim, and Appendix 2 §E.17 records
    /// that referencing local files *"in the util folder"* is what v2.0 replaced
    /// (evidence E26).
    /// </summary>
    /// <remarks>
    /// This renderer emitted <c>../../util/dtd/us-regional-v3-3.dtd</c> until
    /// 2026-08-03, on the reasonable assumption that a regional backbone resolves
    /// its DTD the way the ICH one does. It does not, and only the specification
    /// said so.
    /// <para>
    /// <b>It also puts the epic's Level 2a claim in tension with the format.</b>
    /// FDA wants a network reference; our evidence rests on offline validation
    /// against a pinned DTD. Tests validate a copy with the DOCTYPE rewritten to
    /// the pinned file and assert separately that what ships carries this URL —
    /// so neither the output nor the evidence is bent to suit the other.
    /// </para>
    /// </remarks>
    public const string DoctypeSystemId =
        "https://www.accessdata.fda.gov/static/eCTD/us-regional-v3-3.dtd";

    /// <summary>
    /// Part of the header the specification calls *"always the same"*, and absent
    /// from this renderer until E26 was read.
    /// </summary>
    public const string StylesheetHref =
        "https://www.accessdata.fda.gov/static/eCTD/us-regional.xsl";

    private const string FdaNamespace = "http://www.ich.org/fda";
    private const string XlinkNamespace = "http://www.w3c.org/1999/xlink";
    private const string RootElement = "fda-regional:fda-regional";
    private const string DtdVersion = "3.3";

    /// <summary>
    /// <c>m1-1-forms</c> holds <c>form*</c>, never <c>leaf*</c>, and each form
    /// carries <c>form-type</c> <c>#REQUIRED</c> (evidence E18).
    /// </summary>
    /// <remarks>
    /// Named so the generator can refuse before writing, and refuse in the same
    /// words as ICH's keyed elements — <b>they are one finding</b>
    /// ([ADR-053](../../../docs/adr/ADR-053-instance-qualifiers-belong-to-the-placement.md)).
    /// </remarks>
    public static readonly IReadOnlyDictionary<string, string> KeyedElements =
        new Dictionary<string, string> { ["m1-1-forms"] = "form type" };

    /// <summary>
    /// Module 1 elements the blueprint offers as placement targets and the DTD
    /// declares as <b>containers only</b> — their content models list child
    /// elements and no <c>leaf</c> at all (evidence E19).
    /// </summary>
    /// <remarks>
    /// <b>The blueprint's tree and the backbone's tree disagree about which
    /// nodes hold documents.</b> Of the eight Module 1 sections the FDA IND
    /// blueprint seeds, exactly two accept a leaf — 1.2 Cover Letters and
    /// 1.14.4.1 Investigational Brochure. A document placed in 1.3, 1.4, 1.13 or
    /// 1.14 has nowhere legal to go, and saying so beats emitting a file no
    /// backbone can name.
    /// <para>
    /// Read from the DTD, and listed rather than derived because deriving it
    /// would mean parsing a DTD in <c>src/</c> — and the validator is an oracle,
    /// not a dependency. Extend it when the blueprint seeds more of Module 1.
    /// </para>
    /// </remarks>
    public static readonly IReadOnlySet<string> ContainerOnlyElements =
        new HashSet<string>
        {
            "m1-3-administrative-information",
            "m1-4-references",
            "m1-13-annual-report",
            "m1-14-labeling",
            "m1-14-4-investigational-drug-labeling",
        };

    public static string Render(RegionalBackbone backbone)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\n",
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
        };

        // A stream, not a StringBuilder — the latter makes XmlWriter declare
        // utf-16 whatever the settings say, and the file then announces an
        // encoding its own bytes contradict. S005 found that the hard way.
        using var output = new MemoryStream();

        using (var writer = XmlWriter.Create(output, settings))
        {
            writer.WriteStartDocument(standalone: false);
            writer.WriteDocType(RootElement, null, DoctypeSystemId, null);

            writer.WriteProcessingInstruction(
                "xml-stylesheet", $"type=\"text/xsl\" href=\"{StylesheetHref}\"");

            writer.WriteStartElement("fda-regional", "fda-regional", FdaNamespace);
            writer.WriteAttributeString("xmlns", "xlink", null, XlinkNamespace);
            writer.WriteAttributeString("dtd-version", DtdVersion);

            WriteAdmin(writer, backbone);
            WriteModuleOne(writer, backbone.Leaves);

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        return new UTF8Encoding(false).GetString(output.ToArray());
    }

    /// <summary>
    /// The envelope: who is filing, under which application, as which activity.
    /// <b>Mandatory</b> — <c>fda-regional</c> is <c>(admin, m1-regional?)</c>, so
    /// a sequence with no Module 1 content still carries all of this.
    /// </summary>
    private static void WriteAdmin(XmlWriter writer, RegionalBackbone backbone)
    {
        writer.WriteStartElement("admin");

        writer.WriteStartElement("applicant-info");
        writer.WriteElementString("id", backbone.ApplicantId);
        writer.WriteElementString("company-name", backbone.CompanyName);

        if (backbone.SubmissionDescription is { Length: > 0 } description)
            writer.WriteElementString("submission-description", description);

        writer.WriteStartElement("applicant-contacts");

        foreach (var contact in backbone.Contacts)
        {
            writer.WriteStartElement("applicant-contact");

            writer.WriteStartElement("applicant-contact-name");
            writer.WriteAttributeString(
                "applicant-contact-type", contact.ContactType);
            writer.WriteString(contact.Name);
            writer.WriteEndElement();

            writer.WriteStartElement("telephones");
            foreach (var telephone in contact.Telephones)
            {
                writer.WriteStartElement("telephone");
                writer.WriteAttributeString(
                    "telephone-number-type", telephone.NumberType);
                writer.WriteString(telephone.Number);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();

            writer.WriteStartElement("emails");
            foreach (var email in contact.Emails)
                writer.WriteElementString("email", email);

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndElement();

        writer.WriteStartElement("application-set");
        writer.WriteStartElement("application");

        // Grouped submissions — one sequence filed against several applications
        // — are out of scope, so this sequence's files belong to this
        // application and the answer is never anything but true.
        writer.WriteAttributeString("application-containing-files", "true");

        writer.WriteStartElement("application-information");
        writer.WriteStartElement("application-number");
        writer.WriteAttributeString("application-type", backbone.ApplicationType);
        writer.WriteString(backbone.ApplicationNumber);
        writer.WriteEndElement();
        writer.WriteEndElement();

        writer.WriteStartElement("submission-information");

        // E15, as an attribute rather than a quotation: the activity is named by
        // the sequence that opened it, which is exactly OriginatingSubmissionId.
        writer.WriteStartElement("submission-id");
        writer.WriteAttributeString("submission-type", backbone.SubmissionType);
        writer.WriteString(backbone.SubmissionId);
        writer.WriteEndElement();

        writer.WriteStartElement("sequence-number");
        writer.WriteAttributeString(
            "submission-sub-type", backbone.SubmissionSubType);
        writer.WriteString(backbone.SequenceNumber);
        writer.WriteEndElement();

        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();

        writer.WriteEndElement();
    }

    /// <summary>
    /// Module 1's leaves, each under the element the blueprint names, in the
    /// order the DTD declares.
    /// </summary>
    /// <remarks>
    /// <c>m1-regional</c> is optional, so a sequence whose content is all
    /// Modules 2–5 emits no element at all rather than an empty one.
    /// </remarks>
    private static void WriteModuleOne(
        XmlWriter writer, IReadOnlyList<BackboneLeaf> leaves)
    {
        if (leaves.Count == 0)
            return;

        var placed = leaves.Where(leaf => leaf.ElementPath.Count > 0).ToList();

        if (placed.Count == 0)
            return;

        var root = new Node();

        foreach (var leaf in placed
            .OrderBy(x => x.Href, StringComparer.Ordinal)
            .ThenBy(x => x.Id, StringComparer.Ordinal))
        {
            var node = root;

            foreach (var element in leaf.ElementPath)
                node = node.Child(element);

            node.Leaves.Add(leaf);
        }

        writer.WriteStartElement("m1-regional");
        WriteNode(writer, root);
        writer.WriteEndElement();
    }

    /// <summary>
    /// Children in CTD numbering order, read off the element names themselves.
    /// </summary>
    /// <remarks>
    /// <b>Every content model in this DTD is a sequence, and every one lists its
    /// children in numeric order</b> — <c>m1-1</c>…<c>m1-20</c>,
    /// <c>m1-14-1</c>…<c>m1-14-6</c>. Ordinal string order does not reproduce
    /// that: <c>m1-13-annual-report</c> sorts before <c>m1-2-cover-letters</c>,
    /// and the blueprint seeds both. Comparing the numeric segments does, at
    /// every depth, without a hand-copied list per level to fall out of date.
    /// </remarks>
    private static void WriteNode(XmlWriter writer, Node node)
    {
        foreach (var leaf in node.Leaves)
            WriteLeaf(writer, leaf);

        foreach (var (name, child) in node.Children
            .OrderBy(entry => SectionNumber(entry.Name), NumericSegments.Instance))
        {
            writer.WriteStartElement(name);
            WriteNode(writer, child);
            writer.WriteEndElement();
        }
    }

    /// <summary>The digits in <c>m1-14-4-1-investigational-brochure</c>: 14, 4, 1.</summary>
    private static int[] SectionNumber(string element) =>
        element.Split('-')
            .Skip(1)
            .TakeWhile(segment => int.TryParse(segment, out _))
            .Select(int.Parse)
            .ToArray();

    private sealed class NumericSegments : IComparer<int[]>
    {
        internal static readonly NumericSegments Instance = new();

        public int Compare(int[]? left, int[]? right)
        {
            left ??= [];
            right ??= [];

            for (var i = 0; i < Math.Min(left.Length, right.Length); i++)
            {
                if (left[i] != right[i])
                    return left[i].CompareTo(right[i]);
            }

            return left.Length.CompareTo(right.Length);
        }
    }

    private sealed class Node
    {
        public List<BackboneLeaf> Leaves { get; } = [];

        public List<(string Name, Node Child)> Children { get; } = [];

        public Node Child(string name)
        {
            foreach (var (existing, child) in Children)
            {
                if (existing == name)
                    return child;
            }

            var created = new Node();
            Children.Add((name, created));

            return created;
        }
    }

    /// <summary>
    /// <b>The same leaf, under looser rules — and written as though they were
    /// the strict ones.</b> Here <c>checksum</c> and <c>checksum-type</c> are
    /// <c>#IMPLIED</c>; in <c>index.xml</c> both are <c>#REQUIRED</c> (E16).
    /// </summary>
    /// <remarks>
    /// Emitting them anyway is not belt-and-braces, it is the direction the
    /// asymmetry has to be travelled in. A habit learned from the permissive
    /// backbone — *"checksum is optional"* — produces an invalid
    /// <c>index.xml</c> beside a passing <c>us-regional.xml</c>, and the package
    /// fails only when both are checked together. The habit learned from the
    /// strict one produces a permitted extra attribute here and cannot fail.
    /// </remarks>
    private static void WriteLeaf(XmlWriter writer, BackboneLeaf leaf)
    {
        writer.WriteStartElement("leaf");

        writer.WriteAttributeString("ID", leaf.Id);
        writer.WriteAttributeString("operation", leaf.Operation);

        if (leaf.ModifiedFile is { } modifiedFile)
            writer.WriteAttributeString("modified-file", modifiedFile);

        writer.WriteAttributeString("checksum", leaf.Checksum);
        writer.WriteAttributeString("checksum-type", "md5");
        writer.WriteAttributeString("href", XlinkNamespace, leaf.Href);

        writer.WriteElementString("title", leaf.Title);

        writer.WriteEndElement();
    }
}

/// <summary>
/// Everything <c>us-regional.xml</c> states about a filing, already decided.
/// </summary>
/// <param name="ApplicantId">
/// The applicant's DUNS number.
/// <para>
/// <b>Supplied, never defaulted.</b> A constant here once held FDA's supposedly
/// permitted placeholder <c>999999999</c>, on the strength of a citation to a
/// *Technical Conformance Guide §3.1.1* — a document this repository has never
/// held. Every occurrence of that citation was in a file RegOS wrote. It was
/// removed on 2026-08-03 rather than left to look like evidence.
/// </para>
/// <para>
/// RegOS models no DUNS field, so generation refuses until either the number is
/// modelled or a specification we hold says what to write instead. The renderer
/// can express the value; nothing invents it.
/// </para>
/// </param>
/// <param name="SubmissionId">
/// The sequence number that <b>opened the regulatory activity</b>, four digits.
/// For a sequence that opens one, its own; for a continuation, the opener's.
/// </param>
/// <param name="SequenceNumber">This sequence's own number, four digits.</param>
public sealed record RegionalBackbone(
    string ApplicantId,
    string CompanyName,
    string? SubmissionDescription,
    IReadOnlyList<RegionalContact> Contacts,
    string ApplicationNumber,
    string ApplicationType,
    string SubmissionId,
    string SubmissionType,
    string SequenceNumber,
    string SubmissionSubType,
    IReadOnlyList<BackboneLeaf> Leaves);

/// <param name="ContactType">
/// FDA's <c>applicant-contact-type</c>, translated from RegOS's
/// <c>ContactRole</c> at this boundary. <b>The taxonomies are not merged</b> —
/// ours answers *who is this person to us*, FDA's answers *which box on their
/// side*, and reshaping ours would let one authority redefine the domain model.
/// </param>
public sealed record RegionalContact(
    string Name,
    string ContactType,
    IReadOnlyList<RegionalTelephone> Telephones,
    IReadOnlyList<string> Emails);

public sealed record RegionalTelephone(string Number, string NumberType);
