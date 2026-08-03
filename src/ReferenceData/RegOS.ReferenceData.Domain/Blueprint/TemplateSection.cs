using RegOS.SharedKernel.Exceptions;

namespace RegOS.ReferenceData.Domain.Blueprint;

/// <summary>
/// A node in a template version's dossier tree (e.g. CTD "Module 3", "3.2.S").
/// A child of the aggregate: created only through the template, never
/// independently. It knows only its parent section — required documents attach
/// themselves to it later (STORY-004), it never reaches down to them.
/// </summary>
public sealed class TemplateSection
{
    internal TemplateSection(
        TemplateSectionId id,
        string code,
        string title,
        TemplateSectionId? parentSectionId,
        int order,
        string? ectdFolder = null,
        EctdFolderSource? ectdFolderSource = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException(RegulatoryTemplateErrors.SectionCodeRequired);

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException(RegulatoryTemplateErrors.SectionTitleRequired);

        Id = id;
        // CTD codes carry meaningful casing ("3.2.S" vs "3.2.s") — trim only.
        Code = code.Trim();
        Title = title.Trim();
        ParentSectionId = parentSectionId;
        Order = order;
        EctdFolder = NormaliseFolder(ectdFolder);

        // A folder without a provenance is the thing this model exists to
        // prevent, and a provenance without a folder claims nothing.
        if ((EctdFolder is null) != (ectdFolderSource is null))
            throw new DomainException(
                RegulatoryTemplateErrors.SectionEctdFolderNeedsSource);

        if (ectdFolderSource is { } source && !Enum.IsDefined(source))
            throw new DomainException(
                RegulatoryTemplateErrors.SectionEctdFolderSourceNotRecognised);

        EctdFolderSource = ectdFolderSource;
    }

    public TemplateSectionId Id { get; }

    public string Code { get; }

    /// <summary>
    /// What a regulatory user calls this section — "Annual Report",
    /// "Investigational Brochure".
    /// </summary>
    /// <remarks>
    /// <b>Display, and the authority's to change.</b> FDA restated 1.13 once
    /// already (evidence E9), and wording can move again without a single file
    /// moving with it. Contrast <see cref="EctdFolder"/>, which is part of a
    /// published package's identity: a title is what a person reads, a folder is
    /// where a regulator's tooling looks.
    /// </remarks>
    public string Title { get; }

    // null => top-level module; otherwise the parent section in the same version.
    public TemplateSectionId? ParentSectionId { get; }

    public int Order { get; }

    /// <summary>
    /// Where a document placed in this section is written on disk, relative to
    /// its parent section's folder — or null when the specification that says
    /// so has not been read.
    /// </summary>
    /// <remarks>
    /// <b>A section's folder is versioned regulatory knowledge, not renderer
    /// code.</b> Placement varies with the specification (eCTD 3.2.2 vs 4.0) and
    /// with the authority (<c>m1/us</c> is FDA's), which is exactly the kind of
    /// thing this project keeps as data — a <c>switch</c> in a renderer would be
    /// the mistake RegOS exists not to make.
    /// <para>
    /// <b>Null means the placement is not in evidence</b>, with the same force
    /// as a null <c>Token</c> on the three eCTD catalogues: not "unknown", and
    /// emphatically not "work it out from the section code" — and where RegOS
    /// does choose a name because nothing prescribes one, the choice is labelled
    /// as such (ADR-052).
    /// </para>
    /// <para>
    /// It is <b>one link in a chain, not a whole path</b>. A leaf's location is
    /// the ancestor folders joined. The value may itself contain <c>/</c> where
    /// the specification nests without an intervening section — FDA's Module 1
    /// root is <c>m1/us</c>, one section and two directories.
    /// </para>
    /// <para>
    /// <b>Empty and null are different, and Appendix 4 is why.</b> Sections
    /// 2.7.1 to 2.7.6 have a file row and no directory row: their documents are
    /// written into 2.7's folder, so they contribute nothing of their own. That
    /// is a **known** placement, not a missing one.
    /// <list type="bullet">
    /// <item><c>null</c> — not in evidence. Rendering refuses.</item>
    /// <item><c>""</c> — evidence says this section adds no directory. Rendering
    /// proceeds, using the parent's folder.</item>
    /// <item>a value — this section's own directory.</item>
    /// </list>
    /// Collapsing the first two would make a package impossible to build for
    /// two-thirds of Module 2, for no reason other than a convenience in this
    /// method.
    /// </para>
    /// <para>
    /// <b>Set at construction and never afterwards</b>, because a published
    /// version is frozen (EPIC-007a S002). That has a consequence worth knowing:
    /// filling these in is a *new blueprint version*, not a data patch. It is
    /// the same reasoning ADR-045 §2 gives for freezing the operation — a
    /// package regenerated under a placement rule that changed after
    /// transmission would put files somewhere other than where the authority
    /// received them.
    /// </para>
    /// </remarks>
    public string? EctdFolder { get; }

    /// <summary>
    /// Where <see cref="EctdFolder"/> came from — a specification, or RegOS.
    /// Null exactly when the folder is null.
    /// </summary>
    /// <remarks>
    /// <b>A folder and its provenance travel together or not at all.</b> Storing
    /// a name without saying who chose it would let a value RegOS invented read
    /// exactly like one ICH published — see <see cref="EctdFolderSource"/>.
    /// </remarks>
    public EctdFolderSource? EctdFolderSource { get; }

    /// <summary>
    /// Whether this section's eCTD placement is known — including "known to be
    /// nothing". False only when the specification has not been read.
    /// </summary>
    public bool HasEctdPlacement => EctdFolder is not null;

    /// <summary>
    /// ICH Appendix 2's naming rules, applied per directory segment: lowercase
    /// <c>a-z0-9-</c> only, and at most 64 characters each.
    /// </summary>
    /// <remarks>
    /// Enforced here rather than trusted to the seed, because the value's whole
    /// purpose is to become a filename — and an illegal one is not a cosmetic
    /// defect, it is a package a regulator's tooling rejects.
    /// <para>
    /// <b>Only <c>null</c> travels through as "not in evidence".</b> A supplied
    /// string that trims to nothing is a deliberate statement that the section
    /// adds no directory, and it stays distinguishable from silence.
    /// </para>
    /// </remarks>
    private static string? NormaliseFolder(string? folder)
    {
        if (folder is null)
            return null;

        var trimmed = folder.Trim().Trim('/');

        if (trimmed.Length == 0)
            return string.Empty;

        foreach (var segment in trimmed.Split('/'))
        {
            if (segment.Length is 0 or > MaxFolderSegmentLength
                || !segment.All(c => c is >= 'a' and <= 'z'
                    or >= '0' and <= '9' or '-'))
            {
                throw new DomainException(
                    RegulatoryTemplateErrors.SectionEctdFolderNotLegal);
            }
        }

        return trimmed;
    }

    public const int MaxFolderSegmentLength = 64;
}
