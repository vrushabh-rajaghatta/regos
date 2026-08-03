using System.Text;
using System.Xml;

namespace RegOS.Submission.Application.Generation;

/// <summary>
/// Renders <c>index.xml</c> — the ICH backbone, shared by every region.
/// </summary>
/// <remarks>
/// <b>EPIC-007a S005, and deliberately ignorant of FDA.</b> Nothing here reads a
/// wire token: no <c>submission-type</c>, no <c>submission-sub-type</c>, no
/// <c>application-type</c>. None of S003's vocabulary appears in this file,
/// because none of it appears in the ICH DTD — a renderer reaching for one has
/// reached across a boundary.
/// <para>
/// <b>A pure function of frozen values.</b> It is given leaves and returns text;
/// it opens no database and touches no disk. That is what lets the same package
/// render twice to the same bytes (ADR-049), and what lets the DTD check it in
/// isolation.
/// </para>
/// <para>
/// <b>Module 1 is absent, and that is the story boundary.</b> The DTD makes
/// every module optional, and Module 1's single leaf points at the regional file
/// — <c>us-regional.xml</c> — which S006 writes. Linking a file that does not
/// exist yet is linking to nothing, so the cross-reference is made where the
/// target is made, and S007 is what checks the two halves together.
/// </para>
/// </remarks>
public static class IchBackboneRenderer
{
    /// <summary>The file this renderer produces, at the sequence root.</summary>
    public const string FileName = "index.xml";

    /// <summary>Its checksum, beside it — Appendix 4 #2.</summary>
    public const string ChecksumFileName = "index-md5.txt";

    /// <summary>
    /// The DTD as the package carries it, relative to <c>index.xml</c>. It has
    /// to name what <see cref="SequenceFolderGenerator"/> writes into
    /// <c>util/dtd/</c>, or the package validates against a file it does not
    /// ship.
    /// </summary>
    public const string DoctypeSystemId = "util/dtd/ich-ectd-3-2.dtd";

    /// <summary>
    /// The four elements the DTD will not accept without a business fact
    /// identifying <em>which one</em> — and the attribute each demands.
    /// </summary>
    /// <remarks>
    /// <b>These are not sections; they are keyed, repeatable nodes.</b> Each is
    /// declared <c>*</c> in its parent and carries a <c>#REQUIRED</c> attribute,
    /// because a dossier holds one <c>m3-2-s-drug-substance</c> per substance
    /// per manufacturer and one <c>m5-3-5-…</c> per claimed indication. The
    /// attribute is what tells them apart.
    /// <para>
    /// RegOS's blueprint models 3.2.S as a <em>single</em> section — the
    /// smallest faithful model of the CTD's outline — and the outline is not
    /// what the backbone encodes. Until a placement can say which substance and
    /// whose manufacture it concerns, a leaf underneath one of these cannot be
    /// written down truthfully, so it is refused rather than keyed with an
    /// invented value.
    /// </para>
    /// <para>
    /// The asymmetry is real and worth keeping in view: the drug <em>product</em>
    /// equivalents declare the same attributes <c>#IMPLIED</c>. ICH insists a
    /// substance node be identified and merely permits it for a product.
    /// </para>
    /// </remarks>
    public static readonly IReadOnlyDictionary<string, string> KeyedElements =
        new Dictionary<string, string>
        {
            ["m2-3-s-drug-substance"] = "substance and manufacturer",
            ["m3-2-s-drug-substance"] = "substance and manufacturer",
            ["m2-7-3-summary-of-clinical-efficacy"] = "indication",
            ["m5-3-5-reports-of-efficacy-and-safety-studies"] = "indication",
        };

    private const string EctdNamespace = "http://www.ich.org/ectd";
    private const string XlinkNamespace = "http://www.w3c.org/1999/xlink";
    private const string RootElement = "ectd:ectd";

    /// <summary>
    /// <c>#FIXED "3.2"</c> in the DTD, so it is not a choice — it is the version
    /// of the contract this file claims to satisfy.
    /// </summary>
    private const string DtdVersion = "3.2";

    public static string Render(IReadOnlyList<BackboneLeaf> leaves)
    {
        var root = new ElementNode();

        // Sorted before the tree is built, so two runs place the same leaf in
        // the same position. Ordinal on the joined element path: m2- sorts
        // before m3-, and within a module the CTD's own numbering does the rest.
        foreach (var leaf in leaves
            .OrderBy(x => string.Join('/', x.ElementPath), StringComparer.Ordinal)
            .ThenBy(x => x.Href, StringComparer.Ordinal)
            .ThenBy(x => x.Id, StringComparer.Ordinal))
        {
            var node = root;

            foreach (var element in leaf.ElementPath)
                node = node.Child(element);

            node.Leaves.Add(leaf);
        }

        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            // Fixed rather than Environment.NewLine: a package generated on
            // Windows and one generated here are the same package.
            NewLineChars = "\n",
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
        };

        // A stream, not a StringBuilder. Writing to one makes XmlWriter ignore
        // the encoding above and declare utf-16, because that is what a .NET
        // string is — and the file would then announce an encoding its own
        // bytes contradict. xmllint refuses it outright, which is how this was
        // found rather than shipped.
        using var output = new MemoryStream();

        using (var writer = XmlWriter.Create(output, settings))
        {
            writer.WriteStartDocument(standalone: false);
            writer.WriteDocType(RootElement, null, DoctypeSystemId, null);

            writer.WriteStartElement("ectd", "ectd", EctdNamespace);
            writer.WriteAttributeString(
                "xmlns", "xlink", null, XlinkNamespace);
            writer.WriteAttributeString("dtd-version", DtdVersion);

            WriteNode(writer, root);

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        return new UTF8Encoding(false).GetString(output.ToArray());
    }

    /// <summary>
    /// <b>Leaves before children, always.</b> Every container in the DTD is
    /// declared <c>(leaf*, …)</c> — an ordered sequence, not a choice — so a
    /// leaf written after a child element is invalid however sensible it reads.
    /// </summary>
    private static void WriteNode(XmlWriter writer, ElementNode node)
    {
        foreach (var leaf in node.Leaves)
            WriteLeaf(writer, leaf);

        foreach (var (name, child) in node.Children)
        {
            writer.WriteStartElement(name);
            WriteNode(writer, child);
            writer.WriteEndElement();
        }
    }

    private static void WriteLeaf(XmlWriter writer, BackboneLeaf leaf)
    {
        writer.WriteStartElement("leaf");

        writer.WriteAttributeString("ID", leaf.Id);
        writer.WriteAttributeString("operation", leaf.Operation);

        if (leaf.ModifiedFile is { } modifiedFile)
            writer.WriteAttributeString("modified-file", modifiedFile);

        // Both #REQUIRED. A withdrawal submits no file, so its checksum is the
        // empty string rather than an omission — ICH Appendix 6 Table 6-3 says
        // so in those words, and E16 records that the two backbones disagree
        // about whether the attribute may be dropped at all.
        writer.WriteAttributeString("checksum", leaf.Checksum);
        writer.WriteAttributeString("checksum-type", "md5");

        writer.WriteAttributeString("href", XlinkNamespace, leaf.Href);

        writer.WriteElementString("title", leaf.Title);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Insertion-ordered, because the DTD's content models are sequences and the
    /// blueprint is already in CTD order. A dictionary would be faster and would
    /// emit <c>m2-7</c> before <c>m2-3</c> often enough to be a real defect.
    /// </summary>
    private sealed class ElementNode
    {
        public List<BackboneLeaf> Leaves { get; } = [];

        public List<(string Name, ElementNode Node)> Children { get; } = [];

        public ElementNode Child(string name)
        {
            foreach (var (existing, node) in Children)
            {
                if (existing == name)
                    return node;
            }

            var created = new ElementNode();
            Children.Add((name, created));

            return created;
        }
    }
}

/// <summary>
/// One <c>leaf</c>, with every value already decided.
/// </summary>
/// <param name="ElementPath">
/// The chain of backbone elements this leaf sits under, outermost first.
/// <b>Shared prefixes merge</b> — three sections under 4.2 emit
/// <c>m4-2-study-reports</c> once, not three times.
/// </param>
/// <param name="Id">
/// The stored <c>SubmissionDocumentId</c> with a letter in front of it. An XML
/// <c>ID</c> may not begin with a digit and a GUID often does; nothing else
/// about the value changes, so a leaf is still traceable to the placement it
/// came from.
/// </param>
/// <param name="Operation">
/// <c>new</c>, <c>replace</c>, <c>append</c> or <c>delete</c> — read from
/// <c>SubmissionDocument.Operation</c>, frozen at publish, <b>never recomputed
/// here</b> (ADR-045). <c>Unchanged</c> never reaches this type: it produces no
/// leaf at all, which is the cumulative model meeting the incremental format.
/// </param>
/// <param name="ModifiedFile">
/// Where the superseded leaf lives — <c>../0000/index.xml#leaf-…</c>. Null for
/// anything that supersedes nothing.
/// </param>
public sealed record BackboneLeaf(
    IReadOnlyList<string> ElementPath,
    string Id,
    string Title,
    string Href,
    string Operation,
    string Checksum,
    string? ModifiedFile = null);
