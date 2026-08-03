using System.Text;
using System.Xml;

using RegOS.Submission.Application.StudyTagging;

namespace RegOS.Submission.Application.Generation;

/// <summary>
/// Writes one Study Tagging File — a study-shaped view over leaves
/// <c>index.xml</c> already holds (ADR-054).
/// </summary>
/// <remarks>
/// <b>It carries no files and stores nothing.</b> Every <c>doc-content</c> is a
/// pointer at a leaf ID in the backbone, so an STF deleted from a package can be
/// rebuilt from the sequence — provided the facts it needs are held, which is
/// what EPIC-019 S001–S002b added.
/// <para>
/// <b>Projected from the frozen snapshot, never from the registry.</b> The
/// identifier and title come from what the sequence filed
/// (<c>FiledStudyIdentifier</c>), so regenerating 0000 after a study is renamed
/// reproduces the bytes the authority received:
/// <code>
/// Study (mutable) → Publication → frozen snapshot → STF XML
/// </code>
/// </para>
/// <para>
/// <b>Element order is the DTD's, not ours</b> (E35):
/// <c>study-identifier</c> is <c>(title, study-id, category*)</c> — title
/// first — and <c>study-document</c> follows it. <c>xmllint</c> rejects any
/// other order, which is how that was established rather than assumed.
/// </para>
/// </remarks>
public static class StudyTaggingFileRenderer
{
    public const string DtdFileName = "ich-stf-v2-2.dtd";

    public const string StylesheetFileName = "ich-stf-stylesheet-2-3.xsl";

    public const string DtdVersion = "2.2";

    public const string Namespace = "http://www.ich.org/ectd";

    public const string XlinkNamespace = "http://www.w3.org/1999/xlink";

    /// <param name="toSequenceRoot">
    /// How this STF climbs back to the sequence folder — one <c>../</c> per
    /// folder segment. Everything it points at is relative to where it sits,
    /// and it sits with the study's files rather than at a fixed depth (E29),
    /// so a hardcoded <c>../../</c> is wrong for every section but one. The
    /// first STF ever written proved it: <c>xmllint</c> could not find the DTD.
    /// </param>
    public static string Render(
        PlannedStudyTaggingFile stf, string toSequenceRoot)
    {
        var backboneHref = toSequenceRoot + IchBackboneRenderer.FileName;
        // A MemoryStream, not a StringBuilder: XmlWriter takes its declared
        // encoding from the sink, and a StringBuilder is UTF-16 — so the file
        // would announce utf-16 while the bytes on disk were UTF-8. Caught by
        // xmllint on the first STF ever written, which is what an oracle is
        // for. The two backbone renderers already do it this way.
        var output = new MemoryStream();

        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            OmitXmlDeclaration = false
        };

        using (var writer = XmlWriter.Create(output, settings))
        {
            // The stylesheet, then the doctype — the order every eCTD file in
            // this package uses, and the order a reviewer's tool expects.
            writer.WriteProcessingInstruction(
                "xml-stylesheet",
                $"type=\"text/xsl\" "
                + $"href=\"{toSequenceRoot}util/style/{StylesheetFileName}\"");

            writer.WriteDocType(
                "ectd:study",
                null,
                $"{toSequenceRoot}util/dtd/{DtdFileName}",
                null);

            writer.WriteStartElement("ectd", "study", Namespace);
            writer.WriteAttributeString(
                "xmlns", "xlink", null, XlinkNamespace);
            writer.WriteAttributeString("dtd-version", DtdVersion);

            writer.WriteStartElement("study-identifier");
            writer.WriteElementString("title", stf.Title);
            writer.WriteElementString("study-id", stf.StudyIdentifier);

            // category* is deliberately empty. ICH requires it for exactly four
            // CTD sections, RegOS holds no category facts, and the generator
            // refuses a placement in those four rather than emitting a study
            // described as nothing.
            writer.WriteEndElement();

            writer.WriteStartElement("study-document");

            foreach (var document in stf.Documents)
            {
                writer.WriteStartElement("doc-content");

                writer.WriteAttributeString(
                    "xlink",
                    "href",
                    XlinkNamespace,
                    $"{backboneHref}#{document.LeafId}");

                writer.WriteAttributeString(
                    "xlink", "type", XlinkNamespace, "simple");

                if (document.FileTag is { } tag)
                {
                    writer.WriteStartElement("file-tag");
                    writer.WriteAttributeString("name", tag);

                    // Looked up, never assumed to be "ich": duration and 25
                    // file tags are published under other realms, and the wrong
                    // info-type is a file the DTD accepts and the ICH
                    // stylesheet paints red (E34).
                    writer.WriteAttributeString(
                        "info-type", FileTagVocabulary.RealmOf(tag));

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        return new UTF8Encoding(false).GetString(output.ToArray());
    }
}

/// <param name="StudyIdentifier">
/// The sponsor's code <em>as this sequence filed it</em> — the frozen snapshot,
/// not the registry's current value.
/// </param>
/// <param name="Element">
/// The eCTD element these documents sit in. Part of the grouping key because
/// ICH says one study may generate more than one STF: the mapping is
/// <c>(study, element) → STF</c>, never <c>study → STF</c> (E29 §VI).
/// </param>
/// <param name="Operation">
/// <c>new</c> for the first STF for this pair, <c>append</c> thereafter — the
/// one place eCTD mandates <c>append</c> (E10's third scope).
/// </param>
public sealed record PlannedStudyTaggingFile(
    Guid StudyId,
    string StudyIdentifier,
    string Title,
    string Element,
    string RelativePath,
    string LeafId,
    string Operation,
    string? ModifiedFile,
    IReadOnlyList<TaggedDocument> Documents);

public sealed record TaggedDocument(string LeafId, string? FileTag);
