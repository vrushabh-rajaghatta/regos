using System.Security.Cryptography;
using System.Text;

using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.Organization.Domain.Aggregates.Contact;
using RegOS.ProductDocument.Domain.Entities;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.SharedKernel.Exceptions;
using RegOS.Storage;
using RegOS.Submission.Domain.Submission;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;

namespace RegOS.Submission.Application.Generation;

/// <summary>
/// Writes one published sequence to disk — the folder, the files, and the
/// checksums. <b>No XML.</b>
/// </summary>
/// <remarks>
/// <b>EPIC-007a S004, and the first RegOS code ever to produce part of an eCTD
/// package.</b> It proves one thing and refuses five:
/// <list type="bullet">
/// <item>the same published submission generates <b>byte-identical</b> output
/// every time — which is ADR-049's *"the package is a projection"* stated as
/// something that can fail.</item>
/// <item>a draft, a paper filing, a sequence predating the activity model, one
/// whose vocabulary or placement has never been read, and one whose documents
/// need a fact RegOS does not hold are each refused, in their own words.</item>
/// </list>
/// <para>
/// <b>Nothing is written until everything is checked.</b> A refusal after the
/// first file is on disk leaves a half-built directory that looks like a
/// package, which is worse than no directory at all.
/// </para>
/// </remarks>
public sealed class SequenceFolderGenerator
{
    private readonly RegOSDbContext _dbContext;
    private readonly IFileStorage _storage;

    public SequenceFolderGenerator(
        RegOSDbContext dbContext, IFileStorage storage)
    {
        _dbContext = dbContext;
        _storage = storage;
    }

    /// <param name="destinationRoot">
    /// Where the <c>0000</c> folder is created. <b>Supplied by the caller and
    /// owned by the caller</b> — not a configured location, because a
    /// configured location would quietly become storage, and ADR-049 says v1
    /// stores nothing.
    /// </param>
    public async Task<GeneratedSequenceFolder> GenerateAsync(
        SubmissionId submissionId,
        string destinationRoot,
        CancellationToken cancellationToken = default)
    {
        var submission = await _dbContext.Submissions
            .AsNoTracking()
            .Include(s => s.Documents)
            .Include(s => s.Roles)
            .SingleOrDefaultAsync(s => s.Id == submissionId, cancellationToken)
            ?? throw new NotFoundException(
                SequenceGenerationErrors.SubmissionDoesNotExist);

        var plan = await PlanAsync(submission, cancellationToken);

        return await WriteAsync(plan, destinationRoot, cancellationToken);
    }

    /// <summary>
    /// Everything that can be refused, refused — and everything that will be
    /// written, resolved — before a single byte reaches the disk.
    /// </summary>
    private async Task<SequencePlan> PlanAsync(
        SubmissionAggregate submission, CancellationToken cancellationToken)
    {
        if (submission.Status != SubmissionStatus.Published
            || submission.SequenceNumber is not { } sequenceNumber)
        {
            throw new BusinessRuleViolationException(
                SequenceGenerationErrors.OnlyAFiledSequenceHasAPackage);
        }

        // The proof ADR-047 §4 asked for: the derivation is format-independent,
        // the rendering is not, and paper stops here.
        if (submission.Format != SubmissionFormat.Ectd)
        {
            throw new BusinessRuleViolationException(string.Format(
                SequenceGenerationErrors.OnlyEctdSequencesAreRendered,
                submission.Format));
        }

        // Refusal 1 — our history. Unrecoverable (evidence E13).
        if (!submission.IsClassified)
        {
            throw new BusinessRuleViolationException(
                SequenceGenerationErrors.SequencePredatesTheActivityModel);
        }

        var vocabulary =
            await RequireWireVocabularyAsync(submission, cancellationToken);

        var regional =
            await ResolveRegionalFactsAsync(submission, cancellationToken);

        if (submission.BoundTemplateVersionId is not { } versionId)
        {
            throw new BusinessRuleViolationException(
                SequenceGenerationErrors.SubmissionIsUnbound);
        }

        var placements =
            await ResolveSectionPlacementsAsync(versionId, cancellationToken);

        var priorSequences =
            await ResolvePriorSequencesAsync(submission, cancellationToken);

        var leaves = await ResolveLeavesAsync(
            submission, placements, priorSequences, cancellationToken);

        var deletions = await ResolveDeletionsAsync(
            submission, placements, priorSequences, cancellationToken);

        // Checked here rather than while resolving, because the limit is on the
        // whole path and the sequence folder is part of it.
        foreach (var leaf in leaves)
            RequireAPathTheRegionAccepts(sequenceNumber, leaf.RelativePath);

        var studyTaggingFiles = await ResolveStudyTaggingFilesAsync(
            submission, leaves, cancellationToken);

        foreach (var stf in studyTaggingFiles)
            RequireAPathTheRegionAccepts(sequenceNumber, stf.RelativePath);

        return new SequencePlan(
            sequenceNumber,
            leaves,
            deletions,
            vocabulary,
            regional,
            studyTaggingFiles);
    }

    /// <summary>
    /// Refusal 2 — the authority's vocabulary. Checked here rather than in the
    /// renderer that consumes it, so the failure arrives before the folder
    /// exists.
    /// </summary>
    /// <remarks>
    /// <b>This story writes no XML and still checks the tokens</b>, and that is
    /// deliberate. The rule it states needs no renderer's knowledge — <i>every
    /// reference-data value this sequence points at that carries a token must
    /// have one</i> — and a package half-built before S006 discovers a missing
    /// <c>fdast</c> is a package nobody can act on.
    /// </remarks>
    private async Task<WireVocabulary> RequireWireVocabularyAsync(
        SubmissionAggregate submission, CancellationToken cancellationToken)
    {
        var applicationType = await (
            from application in _dbContext.RegulatoryApplications.AsNoTracking()
            where application.Id == submission.ApplicationId
            join type in _dbContext.ApplicationTypes
                on application.ApplicationTypeId equals type.Id
            select new { type.Code, type.Token }).SingleAsync(cancellationToken);

        RequireToken("Application type", applicationType.Code, applicationType.Token);

        // The activity's type lives on the sequence that opened it — this one,
        // or the one it continues (S003's exclusive-or, read from the far side).
        var typeOwner = submission.OriginatingSubmissionId ?? submission.Id;

        var submissionType = await (
            from sequence in _dbContext.Submissions.AsNoTracking()
            where sequence.Id == typeOwner
            join type in _dbContext.SubmissionTypes
                on sequence.SubmissionTypeId equals type.Id
            select new { type.Code, type.Token }).SingleAsync(cancellationToken);

        RequireToken("Regulatory activity", submissionType.Code, submissionType.Token);

        var subType = await _dbContext.SubmissionSubTypes
            .AsNoTracking()
            .Where(x => x.Id == submission.SubmissionSubTypeId)
            .Select(x => new { x.Code, x.Token })
            .SingleAsync(cancellationToken);

        RequireToken("Sequence action", subType.Code, subType.Token);

        // submission-id borrows the opening sequence's number rather than
        // minting an identity — S003's whole argument for deriving the activity
        // instead of aggregating it (E15, restated by the M1 spec §III.B.2.a).
        var openingSequence = await _dbContext.Submissions
            .AsNoTracking()
            .Where(x => x.Id == typeOwner)
            .Select(x => x.SequenceNumber)
            .SingleAsync(cancellationToken);

        return new WireVocabulary(
            applicationType.Token!,
            submissionType.Token!,
            subType.Token!,
            openingSequence!.Value);

        static void RequireToken(string kind, string code, string? token)
        {
            if (string.IsNullOrEmpty(token))
                throw new BusinessRuleViolationException(string.Format(
                    SequenceGenerationErrors.NoEctdTokenForClassification,
                    kind, code));
        }
    }

    /// <param name="OpeningSequenceNumber">
    /// The sequence that opened the regulatory activity — this one when it opens
    /// its own, and invariant 4 of S003 guarantees no chain to walk.
    /// </param>
    private sealed record WireVocabulary(
        string ApplicationType,
        string SubmissionType,
        string SubmissionSubType,
        int OpeningSequenceNumber);

    /// <summary>
    /// Everything <c>us-regional.xml</c> says about the filing, read from the
    /// domain — <b>and every way it can be refused.</b>
    /// </summary>
    /// <remarks>
    /// <b>Four of the five refusals here are data completeness</b> (ADR-055):
    /// someone knows the answer and has not typed it in, and the message says
    /// what to type and where. That is a different obligation from an unread
    /// specification or an unmodelled concept, and collapsing them would send a
    /// user hunting for a document when they needed a form.
    /// <para>
    /// Resolved during planning, so a missing contact fails before a directory
    /// exists rather than after.
    /// </para>
    /// </remarks>
    private async Task<RegionalFacts> ResolveRegionalFactsAsync(
        SubmissionAggregate submission, CancellationToken cancellationToken)
    {
        var application = await (
            from app in _dbContext.RegulatoryApplications.AsNoTracking()
            where app.Id == submission.ApplicationId
            join type in _dbContext.ApplicationTypes
                on app.ApplicationTypeId equals type.Id
            select new
            {
                app.ApplicantOrganizationId,
                app.ApplicationNumber,
                ApplicationTypeToken = type.Token,
            }).SingleAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(application.ApplicationNumber))
        {
            throw new BusinessRuleViolationException(
                SequenceGenerationErrors.NoApplicationNumberOnTheApplication);
        }

        // FDA's shape, checked at FDA's boundary. The domain stores what the
        // authority assigned and has no opinion about it (ADR-055).
        if (!IsAnFdaApplicationNumber(application.ApplicationNumber))
        {
            throw new BusinessRuleViolationException(string.Format(
                SequenceGenerationErrors.ApplicationNumberIsNotInFdaFormat,
                application.ApplicationNumber));
        }

        var applicant = await _dbContext.Organizations
            .AsNoTracking()
            .Include(x => x.Identifiers)
            .SingleAsync(x => x.Id == application.ApplicantOrganizationId,
                cancellationToken);

        // By code rather than by seeded id: the id lives in a seed class this
        // project cannot see, and the code is the stable public name.
        var dunsSchemeId = await _dbContext.IdentifierSchemes
            .AsNoTracking()
            .Where(x => x.Code == DunsSchemeCode)
            .Select(x => x.Id)
            .SingleOrDefaultAsync(cancellationToken);

        var duns = applicant.Identifiers
            .FirstOrDefault(x => x.SchemeId == dunsSchemeId)?.Value;

        if (string.IsNullOrWhiteSpace(duns))
        {
            throw new BusinessRuleViolationException(string.Format(
                SequenceGenerationErrors.NoDunsNumberForTheApplicant,
                applicant.LegalName));
        }

        if (submission.Title.Length > MaxSubmissionDescription)
        {
            throw new BusinessRuleViolationException(string.Format(
                SequenceGenerationErrors.SubmissionDescriptionTooLong,
                submission.Title.Length, MaxSubmissionDescription));
        }

        return new RegionalFacts(
            duns,
            applicant.LegalName,
            submission.Title,
            await ResolveContactsAsync(submission, cancellationToken),
            application.ApplicationNumber,
            application.ApplicationTypeToken!);
    }

    /// <summary>
    /// The people on the filing (ADR-048), translated into FDA's contact
    /// taxonomy — <b>every one this boundary can express faithfully, and only
    /// those.</b>
    /// </summary>
    /// <remarks>
    /// <b>A role with no translation is skipped, not refused</b>, and
    /// <c>HA-REVIEWER</c> is why: an authority's reviewer is a real person on a
    /// real filing and must never be emitted as one of the applicant's own
    /// contacts. Refusing the package because someone recorded a reviewer would
    /// punish accurate data entry.
    /// <para>
    /// <b>The algorithm is generic; only the table is FDA's.</b> When a role
    /// gains a translation the renderer grows by a row rather than by a branch —
    /// which is the whole reason the taxonomies were never merged.
    /// </para>
    /// </remarks>
    private async Task<IReadOnlyList<RegionalContact>> ResolveContactsAsync(
        SubmissionAggregate submission, CancellationToken cancellationToken)
    {
        var named = submission.Roles.Select(x => x.ContactId).Distinct().ToList();

        var people = await _dbContext.Contacts
            .AsNoTracking()
            .Include(x => x.Emails)
            .Include(x => x.Phones)
            .Where(x => named.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var roleCodes = await _dbContext.ContactRoles
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Code, cancellationToken);

        var contacts = new List<RegionalContact>();

        // Ordered by the code FDA will read, then by name — two runs must
        // produce the same bytes (ADR-049), and a dictionary does not promise
        // that.
        foreach (var assignment in submission.Roles
            .Where(x => roleCodes.ContainsKey(x.RoleId))
            .OrderBy(x => roleCodes[x.RoleId], StringComparer.Ordinal)
            .ThenBy(x => x.ContactId.Value))
        {
            if (!ApplicantContactTypes.TryGetValue(
                    roleCodes[assignment.RoleId], out var contactType))
            {
                continue;
            }

            var person = people[assignment.ContactId];
            var name = $"{person.FirstName} {person.LastName}";

            if (person.Phones.Count == 0 || person.Emails.Count == 0)
            {
                throw new BusinessRuleViolationException(string.Format(
                    SequenceGenerationErrors.ContactIsNotReachable,
                    name,
                    roleCodes[assignment.RoleId],
                    person.Phones.Count == 0
                        ? "telephone number"
                        : "email address"));
            }

            var telephones = new List<RegionalTelephone>();

            foreach (var phone in person.Phones.OrderBy(
                x => x.Number, StringComparer.Ordinal))
            {
                if (phone.Kind is not { } kind)
                {
                    throw new BusinessRuleViolationException(string.Format(
                        SequenceGenerationErrors.PhoneHasNoKind,
                        name, phone.Number));
                }

                telephones.Add(new RegionalTelephone(
                    phone.Number, TelephoneNumberTypes[kind]));
            }

            contacts.Add(new RegionalContact(
                name,
                contactType,
                telephones,
                [.. person.Emails
                    .Select(x => x.Address)
                    .OrderBy(x => x, StringComparer.Ordinal)]));
        }

        if (contacts.Count == 0)
        {
            throw new BusinessRuleViolationException(
                SequenceGenerationErrors.NoRegulatoryContactOnTheSequence);
        }

        return contacts;
    }

    /// <summary>
    /// <c>ContactRole.Code</c> → FDA's <c>applicant-contact-type</c>, and this
    /// table is the entire FDA-specific part of contact translation.
    /// </summary>
    /// <remarks>
    /// <b>One row, and it used to be two.</b> <c>MFG → fdaact2</c> was recorded
    /// on the strength of the M1 specification's phrase *"the technical
    /// contact"*, read as a description; FDA's published list says
    /// <c>fdaact2</c> <em>is</em> the Technical Contact, which is a different
    /// person from a manufacturing one (evidence E31). Nor is <c>fdaact3</c> a
    /// home for an Authorised Representative — a United States Agent is a
    /// specific obligation on a foreign establishment, not the same role under
    /// another flag.
    /// <para>
    /// <c>HA-REVIEWER</c> is absent on purpose and always will be.
    /// </para>
    /// </remarks>
    private static readonly IReadOnlyDictionary<string, string>
        ApplicantContactTypes = new Dictionary<string, string>
        {
            ["REG"] = "fdaact1",
        };

    /// <summary>
    /// The domain's phone kind → FDA's <c>telephone-number-type</c> (E30).
    /// </summary>
    /// <remarks>
    /// <b>One-to-one today, and the correspondence is the world's rather than
    /// the wire's</b> (ADR-055). It lives here, in the boundary, precisely so a
    /// second authority that slices telephones differently changes this table
    /// and not <c>ContactPhone</c>.
    /// </remarks>
    private static readonly IReadOnlyDictionary<ContactPhoneKind, string>
        TelephoneNumberTypes = new Dictionary<ContactPhoneKind, string>
        {
            [ContactPhoneKind.Business] = "fdatnt1",
            [ContactPhoneKind.Fax] = "fdatnt2",
            [ContactPhoneKind.Mobile] = "fdatnt3",
        };

    /// <summary>
    /// *"six (6)-digit … only numeric digits, including any leading zeros …
    /// without letters or dashes"* — M1 Backbone Specification §III.B.1.a.
    /// </summary>
    private static bool IsAnFdaApplicationNumber(string number) =>
        number.Length == FdaApplicationNumberLength && number.All(char.IsAsciiDigit);

    private const int FdaApplicationNumberLength = 6;

    private const string DunsSchemeCode = "DUNS";

    /// <summary>M1 Backbone Specification §III.A.3.</summary>
    private const int MaxSubmissionDescription = 128;

    private sealed record RegionalFacts(
        string ApplicantId,
        string CompanyName,
        string SubmissionDescription,
        IReadOnlyList<RegionalContact> Contacts,
        string ApplicationNumber,
        string ApplicationType);

    /// <summary>
    /// Each section's path inside the sequence folder — its ancestors' folders
    /// joined, in order, skipping the levels that contribute nothing.
    /// </summary>
    /// <remarks>
    /// <b>An empty folder is a level that adds no directory; a null one has
    /// never been read.</b> Appendix 4 gives 2.7.1–2.7.6 a file row and no
    /// directory row — their documents belong in 2.7's folder — so collapsing
    /// the two would make two-thirds of Module 2 unbuildable.
    /// </remarks>
    private async Task<IReadOnlyDictionary<TemplateSectionId, SectionPlacement>>
        ResolveSectionPlacementsAsync(
            RegulatoryTemplateVersionId versionId,
            CancellationToken cancellationToken)
    {
        var sections = await _dbContext.Set<TemplateSection>()
            .AsNoTracking()
            .Where(s => EF.Property<RegulatoryTemplateVersionId>(
                s, "RegulatoryTemplateVersionId") == versionId)
            .ToListAsync(cancellationToken);

        var byId = sections.ToDictionary(s => s.Id);
        var resolved = new Dictionary<TemplateSectionId, SectionPlacement>();

        foreach (var section in sections)
        {
            var segments = new List<string>();
            var elements = new List<string>();
            var regional = new List<string>();

            for (var node = section; node is not null;
                 node = node.ParentSectionId is { } p && byId.TryGetValue(p, out var parent)
                     ? parent : null)
            {
                if (node.EctdFolder is not { } folder)
                {
                    throw new BusinessRuleViolationException(string.Format(
                        SequenceGenerationErrors.NoEctdFolderForSection,
                        node.Code));
                }

                if (node.IchElement is not { } element)
                {
                    throw new BusinessRuleViolationException(string.Format(
                        SequenceGenerationErrors.NoEctdElementForSection,
                        node.Code));
                }

                // The mirror image of the ICH chain, and the split runs the
                // other way: ICH gives Module 1 nothing and the region gives
                // Modules 2-5 nothing, so each is empty exactly where the other
                // is populated. Null still means "not read"; empty means "this
                // backbone says the section has none".
                if (node.RegionalElement is { } regionalElement)
                {
                    if (regionalElement.Length > 0)
                    {
                        regional.InsertRange(0, regionalElement.Split(
                            '/', StringSplitOptions.RemoveEmptyEntries));
                    }
                }
                else if (IsModuleOne(node.Code))
                {
                    throw new BusinessRuleViolationException(string.Format(
                        SequenceGenerationErrors.NoRegionalElementForSection,
                        node.Code));
                }

                if (folder.Length > 0)
                    segments.Insert(0, folder);

                // The same chaining the folder column already does, for the
                // same reason: RegOS's tree is coarser than the CTD's in two
                // places, so one section carries two levels of backbone —
                // m3-2-body-of-data/m3-2-s-drug-substance. An empty value is a
                // section ICH gives no element at all, which is every Module 1
                // sub-section.
                if (element.Length > 0)
                {
                    elements.InsertRange(
                        0, element.Split('/', StringSplitOptions.RemoveEmptyEntries));
                }
            }

            // The renderer owns m1-regional — the DTD gives fda-regional exactly
            // one, so it belongs to the file rather than to any leaf. Leaving it
            // on the chain nests the element inside itself, which is legal-
            // looking XML that no content assertion notices and xmllint rejects.
            if (regional is [FdaRegionalBackboneRenderer.ModuleOneContainer, ..])
                regional.RemoveAt(0);

            resolved[section.Id] = new SectionPlacement(
                section.Code, string.Join('/', segments), elements, regional);
        }

        return resolved;
    }

    /// <summary>
    /// A section's code, not its element — because the element is the thing
    /// being checked for.
    /// </summary>
    private static bool IsModuleOne(string code) =>
        code.StartsWith("1.", StringComparison.Ordinal)
        || code.Equals("M1", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// The sequence folder each superseded or withdrawn placement was filed in
    /// — the <c>../0000/</c> half of a <c>modified-file</c> pointer.
    /// </summary>
    /// <remarks>
    /// A leaf ID is unique within its own sequence and is never reused across
    /// them (ICH Appendix 6), so the pointer needs both halves: which sequence,
    /// and which leaf inside it.
    /// </remarks>
    private async Task<IReadOnlyDictionary<SubmissionDocumentId, int>>
        ResolvePriorSequencesAsync(
            SubmissionAggregate submission,
            CancellationToken cancellationToken)
    {
        var referenced = submission.Documents
            .Select(d => d.ReplacesSubmissionDocumentId)
            .Concat(submission.Deletions
                .Select(d => (SubmissionDocumentId?)d.DeletesSubmissionDocumentId))
            .OfType<SubmissionDocumentId>()
            .Distinct()
            .ToList();

        if (referenced.Count == 0)
            return new Dictionary<SubmissionDocumentId, int>();

        var rows = await (
            from sequence in _dbContext.Submissions.AsNoTracking()
            from document in sequence.Documents
            where referenced.Contains(document.Id)
                && sequence.SequenceNumber != null
            select new { document.Id, sequence.SequenceNumber })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(x => x.Id, x => x.SequenceNumber!.Value);
    }

    /// <summary>
    /// A withdrawal — the one operation with no document behind it.
    /// </summary>
    /// <remarks>
    /// It produces a leaf and <b>no file</b>: ICH Appendix 6 Table 6-3 says
    /// *"there is no new file submitted in this case… the checksum attribute
    /// value will be empty"*. So the two things a leaf normally carries are both
    /// the empty string, and that is the specification's own instruction rather
    /// than a convenience.
    /// </remarks>
    private async Task<PlannedDeletions> ResolveDeletionsAsync(
        SubmissionAggregate submission,
        IReadOnlyDictionary<TemplateSectionId, SectionPlacement> placements,
        IReadOnlyDictionary<SubmissionDocumentId, int> priorSequences,
        CancellationToken cancellationToken)
    {
        if (submission.Deletions.Count == 0)
            return PlannedDeletions.None;

        var names = await NamesOfAsync(
            submission.Deletions.Select(d => d.ProductDocumentId),
            cancellationToken);

        foreach (var deletion in submission.Deletions)
            RequireAWritableBackbonePosition(placements[deletion.TemplateSectionId]);

        // Split by backbone here rather than at write time: a withdrawal in
        // Module 1 is named in us-regional.xml and carries the region's element
        // chain, which the ICH one does not have.
        var withdrawals = submission.Deletions
            .Select(deletion => new
            {
                Placement = placements[deletion.TemplateSectionId],
                Leaf = new BackboneLeaf(
                    placements[deletion.TemplateSectionId].IsRegional
                        ? placements[deletion.TemplateSectionId].RegionalElements
                        : placements[deletion.TemplateSectionId].IchElements,
                    LeafId(deletion.DeletesSubmissionDocumentId),
                    names[deletion.ProductDocumentId],
                    Href: string.Empty,
                    Operation: "delete",
                    Checksum: string.Empty,
                    ModifiedFile: ModifiedFile(
                        deletion.DeletesSubmissionDocumentId,
                        priorSequences,
                        placements[deletion.TemplateSectionId].IsRegional)),
            })
            .OrderBy(x => string.Join('/', x.Leaf.ElementPath), StringComparer.Ordinal)
            .ThenBy(x => x.Leaf.Id, StringComparer.Ordinal)
            .ToList();

        return new PlannedDeletions(
            [.. withdrawals.Where(x => !x.Placement.IsRegional).Select(x => x.Leaf)],
            [.. withdrawals.Where(x => x.Placement.IsRegional).Select(x => x.Leaf)]);
    }

    private sealed record PlannedDeletions(
        IReadOnlyList<BackboneLeaf> Ich,
        IReadOnlyList<BackboneLeaf> Regional)
    {
        public static readonly PlannedDeletions None = new([], []);
    }

    private async Task<Dictionary<
        RegOS.ProductDocument.Domain.IDs.ProductDocumentId, string>> NamesOfAsync(
        IEnumerable<RegOS.ProductDocument.Domain.IDs.ProductDocumentId> ids,
        CancellationToken cancellationToken)
    {
        var wanted = ids.Distinct().ToList();

        return await _dbContext.ProductDocuments.AsNoTracking()
            .Where(document => wanted.Contains(document.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
    }

    /// <summary>
    /// An XML <c>ID</c> may not begin with a digit and a GUID often does, so the
    /// stored id is emitted with a letter in front of it and nothing else
    /// changed — a leaf stays traceable to its placement.
    /// </summary>
    private static string LeafId(SubmissionDocumentId id) =>
        $"leaf-{id.Value:D}";

    private static string? ModifiedFile(
        SubmissionDocumentId? replaces,
        IReadOnlyDictionary<SubmissionDocumentId, int> priorSequences,
        bool regional)
    {
        if (replaces is not { } target
            || !priorSequences.TryGetValue(target, out var sequence))
        {
            return null;
        }

        // Evidence E27. A Module 1 leaf points at the earlier sequence's
        // us-regional.xml, not its index.xml, because that is the file the leaf
        // was named in — and the depth differs with it. The M1 specification
        // §V gives both forms verbatim.
        return regional
            ? $"../../../{sequence:0000}/{FdaRegionalBackboneRenderer.RelativePath}"
                + $"#{LeafId(target)}"
            : $"../{sequence:0000}/{IchBackboneRenderer.FileName}"
                + $"#{LeafId(target)}";
    }

    private async Task<IReadOnlyList<PlannedLeaf>> ResolveLeavesAsync(
        SubmissionAggregate submission,
        IReadOnlyDictionary<TemplateSectionId, SectionPlacement> placements,
        IReadOnlyDictionary<SubmissionDocumentId, int> priorSequences,
        CancellationToken cancellationToken)
    {
        // An operation is a fact about a placement (ADR-045 §5): an attached
        // document that sits in no section produces no leaf and no file.
        //
        // Unchanged is dropped here, and dropping it is the whole thesis. A
        // RegOS sequence holds the entire dossier; an eCTD sequence holds only
        // what changed, and there is no "unchanged" operation to emit. So a
        // carried-forward document produces no leaf — and, because a file
        // nothing references is a file a validator asks about, no file either.
        var placed = submission.Documents
            .Where(d => d.TemplateSectionId is not null
                && d.Operation is not null
                && d.Operation != SubmissionContentOperation.Unchanged)
            .ToList();

        var versionIds = placed.Select(d => d.DocumentVersionId).ToList();

        var versions = await _dbContext.Set<DocumentVersion>()
            .AsNoTracking()
            .Where(v => versionIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, cancellationToken);

        var documentNames = await (
            from document in _dbContext.ProductDocuments.AsNoTracking()
            where placed.Select(p => p.ProductDocumentId).Contains(document.Id)
            select new { document.Id, document.Name })
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var planned = new List<PlannedLeaf>();

        // Ordered before anything is written, and ordered by what the package
        // contains rather than by what the database returned: two runs must
        // produce the same bytes in the same order (ADR-049).
        foreach (var document in placed
            .OrderBy(d => placements[d.TemplateSectionId!.Value].Folder,
                StringComparer.Ordinal)
            .ThenBy(d => documentNames[d.ProductDocumentId], StringComparer.Ordinal)
            .ThenBy(d => d.ProductDocumentId.Value))
        {
            var version = versions[document.DocumentVersionId];
            var placement = placements[document.TemplateSectionId!.Value];

            RequireAWritableBackbonePosition(placement);
            RequireAStudyWhereOneIsOwed(placement, document);

            var folder = placement.Folder;
            var fileName = FileNameFor(version.OriginalFileName);

            var relativePath = folder.Length == 0
                ? fileName
                : $"{folder}/{fileName}";

            if (planned.FirstOrDefault(p => p.RelativePath == relativePath)
                is { } clash)
            {
                throw new BusinessRuleViolationException(string.Format(
                    SequenceGenerationErrors.TwoDocumentsWouldShareAFileName,
                    documentNames[clash.ProductDocumentId],
                    documentNames[document.ProductDocumentId],
                    fileName));
            }

            planned.Add(new PlannedLeaf(
                relativePath,
                document.ProductDocumentId,
                document.DocumentVersionId,
                version.StoragePath,
                placement.IchElements,
                placement.RegionalElements,
                LeafId(document.Id),
                documentNames[document.ProductDocumentId],
                WireOperation(document.Operation!.Value),
                ModifiedFile(
                    document.ReplacesSubmissionDocumentId,
                    priorSequences,
                    placement.IsRegional),
                document.ClinicalStudyId?.Value
                    ?? document.NonClinicalStudyId?.Value,
                document.FiledStudyIdentifier,
                document.FiledStudyTitle,
                document.FileTag,
                folder));
        }

        return planned;
    }

    /// <summary>
    /// One STF per (study, eCTD element) — the projection ADR-054 describes.
    /// </summary>
    /// <remarks>
    /// <b>The grouping key is a pair, not a study</b> (E29 §VI): one study
    /// supporting two CTD subsections files two STFs, and a model keyed on the
    /// study alone would be wrong on the specification's own worked examples.
    /// <para>
    /// <b>Everything comes from the frozen snapshot.</b> The identifier and
    /// title are what <em>this sequence filed</em>, not what the registry says
    /// today, so regenerating a filed sequence reproduces what the authority
    /// received even after the study is renamed. That is the whole freeze
    /// boundary, and it is why this method never touches the Study tables.
    /// </para>
    /// <para>
    /// <b>The <c>append</c> chain is derived, not stored</b> — the same shape
    /// ADR-045 derives a document's operation with, keyed differently: <i>was
    /// there an STF for this pair in a previous published sequence?</i> Nothing
    /// records that an STF existed, because nothing needs to: the placements
    /// that would have produced one are frozen in that sequence.
    /// </para>
    /// </remarks>
    private async Task<IReadOnlyList<PlannedStudyTaggingFile>>
        ResolveStudyTaggingFilesAsync(
            SubmissionAggregate submission,
            IReadOnlyList<PlannedLeaf> leaves,
            CancellationToken cancellationToken)
    {
        var tagged = leaves
            .Where(l => l.StudyId is not null && l.ElementPath.Count > 0)
            .ToList();

        if (tagged.Count == 0) return [];

        var previous = await ResolvePreviousStudyTaggingFilesAsync(
            submission, cancellationToken);

        var planned = new List<PlannedStudyTaggingFile>();

        foreach (var group in tagged
            .GroupBy(l => (StudyId: l.StudyId!.Value, Element: l.ElementPath[^1]))
            .OrderBy(g => g.Key.Element, StringComparer.Ordinal)
            .ThenBy(g => g.Key.StudyId))
        {
            var first = group.First();

            // Refusal — a historical gap. A sequence published before EPIC-019
            // froze study identities has placements that report a study and no
            // record of what it was called at the time. Inventing it from
            // today's registry is exactly what the freeze exists to prevent.
            if (first.FiledStudyIdentifier is not { } identifier
                || first.FiledStudyTitle is not { } title)
            {
                throw new BusinessRuleViolationException(string.Format(
                    SequenceGenerationErrors.SequencePredatesTheStudySnapshot,
                    first.Title));
            }

            RequireAnIdentifierAFileNameCanCarry(identifier);

            // Beside the study's files, as the specification puts it (E29).
            // The group's folders are ordered, so two runs agree.
            var folder = group
                .Select(l => l.Folder)
                .OrderBy(f => f, StringComparer.Ordinal)
                .First();

            var fileName = $"stf-{identifier.ToLowerInvariant()}.xml";

            var relativePath = folder.Length == 0
                ? fileName
                : $"{folder}/{fileName}";

            var key = (group.Key.StudyId, group.Key.Element);

            var priorSequence = previous.TryGetValue(key, out var sequence)
                ? sequence
                : (int?)null;

            planned.Add(new PlannedStudyTaggingFile(
                group.Key.StudyId,
                identifier,
                title,
                group.Key.Element,
                relativePath,
                StudyTaggingLeafId(group.Key.StudyId, group.Key.Element),
                // The one place eCTD mandates append (E10's third scope).
                priorSequence is null ? "new" : "append",
                priorSequence is { } number
                    ? $"../../../{number:0000}/{relativePath}"
                        + $"#{StudyTaggingLeafId(group.Key.StudyId, group.Key.Element)}"
                    : null,
                group
                    .OrderBy(l => l.RelativePath, StringComparer.Ordinal)
                    .Select(l => new TaggedDocument(l.LeafId, l.FileTag))
                    .ToList()));
        }

        return planned;
    }

    /// <summary>
    /// The most recent published sequence that filed an STF for each (study,
    /// element) — derived from its placements, because an STF is a projection
    /// and nothing stores that one existed.
    /// </summary>
    private async Task<IReadOnlyDictionary<(Guid, string), int>>
        ResolvePreviousStudyTaggingFilesAsync(
            SubmissionAggregate submission,
            CancellationToken cancellationToken)
    {
        var earlier = await _dbContext.Set<SubmissionAggregate>()
            .AsNoTracking()
            .Include(s => s.Documents)
            .Where(s => s.ApplicationId == submission.ApplicationId
                && s.SequenceNumber != null
                && s.Id != submission.Id)
            .ToListAsync(cancellationToken);

        if (earlier.Count == 0) return new Dictionary<(Guid, string), int>();

        var sections = await _dbContext.Set<TemplateSection>()
            .AsNoTracking()
            .Where(x => x.IchElement != null)
            .ToDictionaryAsync(x => x.Id, x => x.IchElement!, cancellationToken);

        var chain = new Dictionary<(Guid, string), int>();

        foreach (var sequence in earlier.OrderBy(s => s.SequenceNumber))
        {
            foreach (var document in sequence.Documents)
            {
                var studyId = document.ClinicalStudyId?.Value
                    ?? document.NonClinicalStudyId?.Value;

                if (studyId is not { } id
                    || document.TemplateSectionId is not { } sectionId
                    || document.Operation is null
                    || document.Operation == SubmissionContentOperation.Unchanged)
                {
                    continue;
                }

                if (!sections.TryGetValue(sectionId, out var element)
                    || string.IsNullOrEmpty(element))
                {
                    continue;
                }

                // Latest wins: a third sequence appends to the second, never to
                // the first — "you should not continually append to the
                // original STF" (E29 §V).
                chain[(id, element)] = sequence.SequenceNumber!.Value;
            }
        }

        return chain;
    }

    /// <summary>
    /// Deterministic, and derived from the pair rather than from a row: a later
    /// sequence has to point at this leaf, and regenerating must produce the
    /// same id (ADR-054 §5).
    /// </summary>
    private static string StudyTaggingLeafId(Guid studyId, string element) =>
        $"leaf-stf-{Slug(element)}-{studyId:N}";

    /// <summary>
    /// FDA accepts no path longer than 150 characters (evidence E22).
    /// </summary>
    /// <remarks>
    /// <b>The stricter of two published limits.</b> ICH Appendix 2 allows 230,
    /// so a path this refuses may be perfectly legal elsewhere — which is why the
    /// message names the region rather than the format.
    /// <para>
    /// <b>What is measured is what RegOS emits</b>: the sequence folder and
    /// everything under it, <c>0000/m3/32-body-data/…</c>. The application folder
    /// above it (<c>NDA123456/</c>) is the caller's, chosen at delivery, and it
    /// spends from the same 150 — so a package that passes here can still exceed
    /// the limit once it is placed. Measuring what we do not choose would be
    /// guessing; leaving the rest unsaid would be worse.
    /// </para>
    /// </remarks>
    private static void RequireAPathTheRegionAccepts(
        int sequenceNumber, string relativePath)
    {
        var path = $"{sequenceNumber:0000}/{relativePath}";

        if (path.Length > MaxPathLength)
        {
            throw new BusinessRuleViolationException(string.Format(
                SequenceGenerationErrors.PathTooLongForTheRegion,
                path, path.Length, MaxPathLength));
        }
    }

    /// <summary>FDA eCTD Technical Conformance Guide §2.4.</summary>
    private const int MaxPathLength = 150;

    /// <summary>
    /// Refusal 3 — the specification asks for a fact RegOS does not hold.
    /// </summary>
    /// <remarks>
    /// Checked while planning, so it lands before any file exists, and checked
    /// per <em>placed</em> section rather than per seeded one: a blueprint that
    /// merely offers 3.2.S is fine, and it is putting a document there that
    /// cannot be written down.
    /// </remarks>
    private static void RequireAWritableBackbonePosition(
        SectionPlacement placement)
    {
        foreach (var element in placement.IchElements)
        {
            if (IchBackboneRenderer.KeyedElements.TryGetValue(element, out var key))
            {
                throw new BusinessRuleViolationException(string.Format(
                    SequenceGenerationErrors.SectionNeedsAFactRegOsDoesNotHold,
                    placement.Code, element, key));
            }
        }

        if (!placement.IsRegional)
            return;

        // E19 — the blueprint's tree and the backbone's tree disagree about
        // which nodes hold documents, and only the region's DTD knows. Checked
        // on the innermost element, which is where the leaf would go.
        if (placement.RegionalElements is [.., var innermost])
        {
            if (FdaRegionalBackboneRenderer.KeyedElements
                .TryGetValue(innermost, out var regionalKey))
            {
                throw new BusinessRuleViolationException(string.Format(
                    SequenceGenerationErrors.SectionNeedsAFactRegOsDoesNotHold,
                    placement.Code, innermost, regionalKey));
            }

            if (FdaRegionalBackboneRenderer.ContainerOnlyElements
                .Contains(innermost))
            {
                throw new BusinessRuleViolationException(string.Format(
                    SequenceGenerationErrors.SectionHoldsNoDocuments,
                    placement.Code, innermost));
            }
        }
    }

    /// <summary>
    /// Refusal 3 again — and this one covers a whole module.
    /// </summary>
    /// <remarks>
    /// FDA requires a Study Tagging File for every file in <c>4.2.x</c> and
    /// <c>5.3.1.x–5.3.5.x</c> (evidence E21). An STF names the study a document
    /// belongs to; RegOS records no studies, so <b>ADR-054</b> says generation
    /// refuses by name until one is modelled.
    /// <para>
    /// <b>Matched on the backbone element, not on the section code.</b> The
    /// range FDA gives is a range of CTD section numbers, and an ICH element
    /// name carries its section number at the front — <c>m4-2-3-toxicology</c> —
    /// so the prefix <em>is</em> the number. A blueprint code is ours and could
    /// be written any way; an element name is the DTD's and cannot.
    /// </para>
    /// <para>
    /// <b>The bounds are the ones FDA drew, not the modules'.</b> 5.2 is exempt
    /// by name, bare 5.3 is outside the enumerated range, and 5.3.6 and 5.3.7
    /// are past its end. A rule that refused all of Modules 4 and 5 would look
    /// identical on every document the blueprint seeds today and be wrong.
    /// </para>
    /// <para>
    /// <b>A withdrawal is exempt, and the specification says so outright:</b>
    /// deleting a study document deletes the leaf in <c>index.xml</c> and
    /// submits no STF (E29). That is why this is checked where leaves are
    /// resolved and not where deletions are — the omission is the rule.
    /// </para>
    /// </remarks>
    private static void RequireAStudyWhereOneIsOwed(
        SectionPlacement placement, SubmissionDocument document)
    {
        // Empty for every Module 1 section — ICH's m1 is (leaf*) and gives them
        // no element at all.
        if (placement.IchElements.Count == 0)
            return;

        // The innermost element of the chain is the one the leaf is written
        // into, and ICH names a child of 4.2.x m4-2-… itself — so testing the
        // innermost tests the chain, and naming it names where the leaf went
        // rather than the container above it.
        var element = placement.IchElements[^1];

        if (!StudyTaggedSections.Any(
            prefix => element.StartsWith(prefix, StringComparison.Ordinal)))
        {
            return;
        }

        // Refusal — data completeness. The capability exists now; what is
        // missing is a fact a user can supply on the content plan.
        if (!document.ReportsAStudy)
        {
            throw new BusinessRuleViolationException(string.Format(
                SequenceGenerationErrors.SectionRequiresAStudyTaggingFile,
                placement.Code, element));
        }

        // Refusal — domain capability. ICH requires species, route, duration
        // and type-of-control for exactly these four sections, and a Study
        // holds none of them (ADR-056 §3 admits an attribute when a workflow
        // demands it — this is that demand, and it is a story rather than a
        // guess).
        if (CategorySections.Any(
            prefix => element.StartsWith(prefix, StringComparison.Ordinal)))
        {
            throw new BusinessRuleViolationException(string.Format(
                SequenceGenerationErrors.SectionRequiresStudyCategories,
                placement.Code, element));
        }
    }

    /// <summary>
    /// The four CTD sections whose STF must carry <c>category</c> — species,
    /// route-of-admin, duration, type-of-control (E29, E33).
    /// </summary>
    /// <remarks>
    /// <b>Refused rather than emitted empty.</b> <c>category*</c> is optional in
    /// the DTD, so an STF without one is structurally valid and would pass
    /// <c>xmllint</c> — and would tell a reviewer nothing about a study they are
    /// required to be told about. This is E23's shape again: legal, and wrong.
    /// <para>
    /// The seeded FDA IND blueprint offers none of these four, so nothing today
    /// can reach this. It is written now because the alternative is discovering
    /// it when a blueprint gains 4.2.3.1.
    /// </para>
    /// </remarks>
    private static readonly string[] CategorySections =
        ["m4-2-3-1-", "m4-2-3-2-", "m4-2-3-4-1-", "m5-3-5-1-"];

    /// <summary>
    /// An STF is named <c>stf-&lt;study-id&gt;.xml</c> (E29), so the sponsor's
    /// code becomes a filename.
    /// </summary>
    /// <remarks>
    /// <b>Refused, never slugged.</b> A slug would put a name in the package
    /// that is not the study's — and the filename is one of the things a
    /// reviewer reads. S001 predicted this refusal when it declined to police
    /// the identifier's format in the domain: the check belongs at the boundary
    /// that needs it, and this is that boundary.
    /// </remarks>
    private static void RequireAnIdentifierAFileNameCanCarry(string identifier)
    {
        if (identifier.All(c =>
            char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.'))
        {
            return;
        }

        throw new BusinessRuleViolationException(string.Format(
            SequenceGenerationErrors.StudyIdentifierIsNotAFileName, identifier));
    }

    /// <summary>
    /// *"Study Tagging Files (STFs) are required for all files in section 4.2.x
    /// and 5.3.1.x – 5.3.5.x"* — FDA eCTD Technical Conformance Guide §2.8,
    /// written as the element prefixes those sections carry.
    /// </summary>
    private static readonly string[] StudyTaggedSections =
        ["m4-2-", "m5-3-1-", "m5-3-2-", "m5-3-3-", "m5-3-4-", "m5-3-5-"];

    /// <summary>
    /// <c>operation (new | append | replace | delete)</c> — the DTD's own
    /// enumeration, closed and exhaustive (evidence E14).
    /// </summary>
    /// <remarks>
    /// <b>Read, never recomputed</b> (ADR-045). The value was decided at publish
    /// against the dossier as it then stood; deriving it again here would let a
    /// later rule change quietly rewrite what a filed sequence said.
    /// </remarks>
    private static string WireOperation(SubmissionContentOperation operation)
        => operation switch
        {
            SubmissionContentOperation.New => "new",
            SubmissionContentOperation.Replace => "replace",
            SubmissionContentOperation.Append => "append",

            // Unchanged never reaches here — it produces no leaf. Delete has no
            // SubmissionDocument behind it and arrives as a SubmissionDeletion.
            _ => throw new InvalidOperationException(
                $"'{operation}' does not describe a leaf in a filed sequence."),
        };

    /// <summary>
    /// ICH Appendix 2 applied to a file name: lowercase, <c>a-z0-9-</c>, and the
    /// extension kept.
    /// </summary>
    /// <remarks>
    /// A pure function of the stored name, so the same document always produces
    /// the same file — which is half of why regeneration is byte-identical.
    /// </remarks>
    public static string FileNameFor(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName).TrimStart('.');
        var stem = Path.GetFileNameWithoutExtension(originalFileName);

        var slug = Slug(stem);
        if (slug.Length == 0)
            slug = "document";

        return extension.Length == 0 ? slug : $"{slug}.{Slug(extension)}";
    }

    private static string Slug(string value)
    {
        var builder = new StringBuilder(value.Length);

        foreach (var c in value.ToLowerInvariant())
        {
            if (c is >= 'a' and <= 'z' or >= '0' and <= '9')
                builder.Append(c);
            else if (builder.Length > 0 && builder[^1] != '-')
                builder.Append('-');
        }

        return builder.ToString().Trim('-');
    }

    private async Task<GeneratedSequenceFolder> WriteAsync(
        SequencePlan plan,
        string destinationRoot,
        CancellationToken cancellationToken)
    {
        // 0000, not 0001 — the number RegOS holds (evidence E4). Every FDA
        // example starts at 0001 (E5), and that comparison belongs to S008.
        var root = Path.Combine(
            destinationRoot, plan.SequenceNumber.ToString("0000"));

        Directory.CreateDirectory(root);

        var leaves = new List<GeneratedLeaf>(plan.Leaves.Count);

        foreach (var leaf in plan.Leaves)
        {
            var absolute = Path.Combine(
                root, leaf.RelativePath.Replace('/', Path.DirectorySeparatorChar));

            Directory.CreateDirectory(Path.GetDirectoryName(absolute)!);

            await using var source =
                await _storage.OpenReadAsync(leaf.StoragePath, cancellationToken);
            await using var buffer = new MemoryStream();
            await source.CopyToAsync(buffer, cancellationToken);

            var bytes = buffer.ToArray();
            await File.WriteAllBytesAsync(absolute, bytes, cancellationToken);

            leaves.Add(new GeneratedLeaf(
                leaf.RelativePath,
                leaf.ProductDocumentId,
                leaf.DocumentVersionId,
                Convert.ToHexString(MD5.HashData(bytes)).ToLowerInvariant(),
                bytes.LongLength));
        }

        var utilities = await WriteDtdsAsync(root, cancellationToken);

        // Written before index.xml, because the backbone quotes their
        // checksums — and after the leaves, because each one points at leaf IDs
        // the backbone will carry.
        var taggingFiles = await WriteStudyTaggingFilesAsync(
            root, plan, cancellationToken);

        // The regional file first, because index.xml's Module 1 leaf points at
        // it — and a backbone that links a file which does not exist is worse
        // than one that links nothing (S005 deferred the link for that reason).
        var regional = await WriteRegionalBackboneAsync(
            root, plan, leaves, cancellationToken);

        var backbones = new List<string> { regional.Path };

        backbones.AddRange(await WriteIchBackboneAsync(
            root, plan, leaves, taggingFiles, regional.Md5, cancellationToken));

        // Every emitted path, not only the leaves. These are fixed strings and
        // the check cannot fire today — which is the point: if a DTD is renamed
        // or a backbone moves, the limit is still enforced rather than assumed.
        foreach (var path in utilities.Concat(backbones))
            RequireAPathTheRegionAccepts(plan.SequenceNumber, path);

        return new GeneratedSequenceFolder(
            root,
            leaves,
            utilities,
            backbones,
            taggingFiles.Select(f => f.Path).ToList());
    }

    /// <summary>
    /// <c>index.xml</c> and <c>index-md5.txt</c>, written last because the
    /// backbone quotes the checksum of every file beneath it.
    /// </summary>
    /// <remarks>
    /// <b>Module 1 is held back for S006.</b> ICH's
    /// <c>m1-administrative-information-and-prescribing-information</c> is
    /// declared <c>(leaf*)</c> with no children of its own — ICH defers the
    /// whole module to the regions — so a Module 1 document has no ICH element
    /// to sit under. Its leaves belong to the regional backbone, and the one
    /// leaf ICH's m1 does carry points at that file, which does not exist yet.
    /// </remarks>
    private static async Task<IReadOnlyList<string>> WriteIchBackboneAsync(
        string root,
        SequencePlan plan,
        IReadOnlyList<GeneratedLeaf> written,
        IReadOnlyList<(string Path, string Md5)> taggingFiles,
        string regionalBackboneMd5,
        CancellationToken cancellationToken)
    {
        var checksums = written.ToDictionary(x => x.RelativePath, x => x.Md5);

        foreach (var (path, md5) in taggingFiles)
            checksums[path] = md5;

        var leaves = plan.Leaves
            .Where(leaf => leaf.RegionalElementPath.Count == 0)
            .Select(leaf => new BackboneLeaf(
                leaf.ElementPath,
                leaf.LeafId,
                leaf.Title,
                leaf.RelativePath,
                leaf.Operation,
                checksums[leaf.RelativePath],
                leaf.ModifiedFile))
            // The STF's own leaf, in the element its documents sit in — an STF
            // is content of the section it describes, not a utility file.
            .Concat(plan.StudyTaggingFiles.Select(stf => new BackboneLeaf(
                ElementChainFor(plan, stf),
                stf.LeafId,
                $"Study Tagging File — {stf.StudyIdentifier}",
                stf.RelativePath,
                stf.Operation,
                checksums[stf.RelativePath],
                stf.ModifiedFile)))
            .Concat(plan.Deletions.Ich)
            .Append(ModuleOneCrossLink(regionalBackboneMd5))
            .ToList();

        var xml = IchBackboneRenderer.Render(leaves);

        // UTF-8 without a BOM. The declaration says UTF-8 and a BOM would be a
        // second, silent claim about the same thing.
        var bytes = new UTF8Encoding(false).GetBytes(xml);

        await File.WriteAllBytesAsync(
            Path.Combine(root, IchBackboneRenderer.FileName),
            bytes,
            cancellationToken);

        await File.WriteAllTextAsync(
            Path.Combine(root, IchBackboneRenderer.ChecksumFileName),
            Convert.ToHexString(MD5.HashData(bytes)).ToLowerInvariant(),
            cancellationToken);

        return [IchBackboneRenderer.FileName, IchBackboneRenderer.ChecksumFileName];
    }

    /// <summary>
    /// <c>m1/us/us-regional.xml</c> — everything ICH defers to the region, plus
    /// the administrative block that identifies the filing.
    /// </summary>
    /// <remarks>
    /// <b>Its own renderer, not a shared one with a flag</b> (E16). The two
    /// backbones disagree on whether a leaf's checksum is required, and a single
    /// <c>renderLeaf</c> satisfying the looser rule produces a valid
    /// <c>us-regional.xml</c> beside an invalid <c>index.xml</c> — the worst
    /// shape a defect can have, because the evidence points at the wrong file.
    /// What they share is the projection beneath them, which is
    /// <c>plan.Leaves</c>.
    /// </remarks>
    private static async Task<(string Path, string Md5)> WriteRegionalBackboneAsync(
        string root,
        SequencePlan plan,
        IReadOnlyList<GeneratedLeaf> written,
        CancellationToken cancellationToken)
    {
        var checksums = written.ToDictionary(x => x.RelativePath, x => x.Md5);

        var leaves = plan.Leaves
            .Where(leaf => leaf.RegionalElementPath.Count > 0)
            .Select(leaf => new BackboneLeaf(
                leaf.RegionalElementPath,
                leaf.LeafId,
                leaf.Title,
                // Relative to m1/us/, where this file sits — two levels up
                // from the sequence root the ICH backbone measures from.
                RelativeToRegional(leaf.RelativePath),
                leaf.Operation,
                checksums[leaf.RelativePath],
                leaf.ModifiedFile))
            .Concat(plan.Deletions.Regional)
            .ToList();

        var backbone = new RegionalBackbone(
            plan.Regional.ApplicantId,
            plan.Regional.CompanyName,
            plan.Regional.SubmissionDescription,
            plan.Regional.Contacts,
            plan.Regional.ApplicationNumber,
            plan.Vocabulary.ApplicationType,
            $"{plan.Vocabulary.OpeningSequenceNumber:0000}",
            plan.Vocabulary.SubmissionType,
            $"{plan.SequenceNumber:0000}",
            plan.Vocabulary.SubmissionSubType,
            leaves);

        var relative = FdaRegionalBackboneRenderer.RelativePath;
        var absolute = Path.Combine(
            root, relative.Replace('/', Path.DirectorySeparatorChar));

        Directory.CreateDirectory(Path.GetDirectoryName(absolute)!);

        var bytes = new UTF8Encoding(false).GetBytes(
            FdaRegionalBackboneRenderer.Render(backbone));

        await File.WriteAllBytesAsync(absolute, bytes, cancellationToken);

        // Hashed here because index.xml's cross-link quotes it, and ICH makes a
        // leaf's checksum #REQUIRED. Writing this file before the ICH one is
        // what makes that possible.
        return (relative,
            Convert.ToHexString(MD5.HashData(bytes)).ToLowerInvariant());
    }

    /// <summary>
    /// A leaf's <c>xlink:href</c> is relative to the backbone that names it, and
    /// <c>us-regional.xml</c> sits two levels below the sequence root.
    /// </summary>
    private static string RelativeToRegional(string sequenceRelativePath) =>
        $"../../{sequenceRelativePath}";

    /// <summary>
    /// The one leaf ICH's <c>m1</c> carries: a pointer at the regional backbone.
    /// </summary>
    /// <remarks>
    /// <b>S005 deferred this deliberately</b> — *"a backbone that links a
    /// missing file is worse than one that links nothing"* — and it is written
    /// now because <c>us-regional.xml</c> exists by the time this runs.
    /// <para>
    /// <b>It is the seam S007 exists to check.</b> Each file validates alone
    /// whether or not this link resolves; only a validator pointed at the whole
    /// package can tell.
    /// </para>
    /// <para>
    /// A fixed id and a fixed operation: the regional backbone is not a document
    /// with a lifecycle, it is the other half of this sequence's own structure,
    /// and every sequence carries a current one. ICH's <c>m1</c> is
    /// <c>(leaf*)</c>, so this is the only shape available.
    /// </para>
    /// </remarks>
    private static BackboneLeaf ModuleOneCrossLink(string md5) => new(
        [ModuleOneElement],
        "leaf-m1-regional",
        "Module 1 — Administrative Information and Prescribing Information",
        FdaRegionalBackboneRenderer.RelativePath,
        "new",
        Checksum: md5);

    /// <summary>
    /// ICH's Module 1 element. Named here because index.xml is the file that has
    /// to leave it alone, not because this renderer knows anything about a
    /// region.
    /// </summary>
    private const string ModuleOneElement =
        "m1-administrative-information-and-prescribing-information";

    /// <summary>
    /// <c>util/dtd/</c> — the DTDs the package must carry (Appendix 4 #371–376).
    /// </summary>
    /// <remarks>
    /// Only the region being filed to needs its regional DTD, which #371 states
    /// outright, so one regional file rather than four.
    /// <para>
    /// <b>The names are the published ones, not Appendix 4's pattern.</b> #371
    /// says rows 372–379 are *"illustrative only … consult regional guidance for
    /// the current name and version"* — so the pattern <c>us-regional-3-3.dtd</c>
    /// suggests is exactly what the appendix disclaims, and FDA publishes
    /// <c>us-regional-v3-3.dtd</c>. This is not ADR-052's territory: that
    /// governs names <em>nobody</em> published, and this one is published.
    /// </para>
    /// <para>
    /// The DOCTYPE each backbone emits, the embedded resource, and the file on
    /// disk all name the same string. A package that validates against a DTD it
    /// does not carry is the failure a reviewer cannot see and a regulator can.
    /// </para>
    /// </remarks>
    /// <summary>
    /// One file per (study, element), written from the frozen snapshot.
    /// </summary>
    private static async Task<IReadOnlyList<(string Path, string Md5)>>
        WriteStudyTaggingFilesAsync(
            string root,
            SequencePlan plan,
            CancellationToken cancellationToken)
    {
        var written = new List<(string Path, string Md5)>();

        foreach (var stf in plan.StudyTaggingFiles)
        {
            var absolute = Path.Combine(
                root, stf.RelativePath.Replace('/', Path.DirectorySeparatorChar));

            Directory.CreateDirectory(Path.GetDirectoryName(absolute)!);

            // How this file climbs back to the sequence folder: one ".." per
            // folder segment. The backbone, the DTD and the stylesheet are all
            // relative to it, and an STF sits with the study's files rather
            // than at a fixed depth — so this is computed, never assumed.
            var depth = stf.RelativePath.Count(c => c == '/');

            var toRoot = string.Concat(Enumerable.Repeat("../", depth));

            var xml = StudyTaggingFileRenderer.Render(stf, toRoot);

            var bytes = new UTF8Encoding(false).GetBytes(xml);

            await File.WriteAllBytesAsync(absolute, bytes, cancellationToken);

            written.Add((
                stf.RelativePath,
                Convert.ToHexString(MD5.HashData(bytes)).ToLowerInvariant()));
        }

        return written;
    }

    /// <summary>
    /// The element chain an STF's leaf sits in — the same chain its documents
    /// use, read from any one of them.
    /// </summary>
    private static IReadOnlyList<string> ElementChainFor(
        SequencePlan plan, PlannedStudyTaggingFile stf) =>
        plan.Leaves
            .First(l => l.StudyId == stf.StudyId
                && l.ElementPath.Count > 0
                && l.ElementPath[^1] == stf.Element)
            .ElementPath;

    private static async Task<IReadOnlyList<string>> WriteDtdsAsync(
        string root, CancellationToken cancellationToken)
    {
        var written = new List<string>();

        // The STF DTD joins the two backbone DTDs, and the stylesheet joins it
        // with the vocabulary it reads — util/style/ is the folder ADR-054
        // recorded as absent without knowing what went in it.
        foreach (var (name, folder) in new[]
        {
            ("ich-ectd-3-2.dtd", "dtd"),
            ("us-regional-v3-3.dtd", "dtd"),
            ("ich-stf-v2-2.dtd", "dtd"),
            ("ich-stf-stylesheet-2-3.xsl", "style"),
            ("valid-values.xml", "style")
        })
        {
            var relative = $"util/{folder}/{name}";
            var absolute = Path.Combine(
                root, relative.Replace('/', Path.DirectorySeparatorChar));

            Directory.CreateDirectory(Path.GetDirectoryName(absolute)!);

            await using var resource = typeof(SequenceFolderGenerator).Assembly
                .GetManifestResourceStream(
                    $"RegOS.Submission.Application.Generation.{name}")
                ?? throw new InvalidOperationException(
                    $"The eCTD artifact '{name}' is not embedded in this build.");

            await using var file = File.Create(absolute);
            await resource.CopyToAsync(file, cancellationToken);

            written.Add(relative);
        }

        return written;
    }

    private sealed record SequencePlan(
        int SequenceNumber,
        IReadOnlyList<PlannedLeaf> Leaves,
        PlannedDeletions Deletions,
        WireVocabulary Vocabulary,
        RegionalFacts Regional,
        IReadOnlyList<PlannedStudyTaggingFile> StudyTaggingFiles);

    private sealed record PlannedLeaf(
        string RelativePath,
        RegOS.ProductDocument.Domain.IDs.ProductDocumentId ProductDocumentId,
        RegOS.ProductDocument.Domain.IDs.DocumentVersionId DocumentVersionId,
        string StoragePath,
        IReadOnlyList<string> ElementPath,
        IReadOnlyList<string> RegionalElementPath,
        string LeafId,
        string Title,
        string Operation,
        string? ModifiedFile,
        Guid? StudyId = null,
        string? FiledStudyIdentifier = null,
        string? FiledStudyTitle = null,
        string? FileTag = null,
        string Folder = "");

    /// <summary>
    /// Where a section's documents go — on disk, and in the backbone. Two
    /// answers to *"where does this belong?"*, resolved together because they
    /// walk the same ancestor chain.
    /// </summary>
    private sealed record SectionPlacement(
        string Code,
        string Folder,
        IReadOnlyList<string> IchElements,
        IReadOnlyList<string> RegionalElements)
    {
        /// <summary>
        /// Which backbone names this leaf. <b>Exactly one does</b> — ICH's
        /// Module 1 is <c>(leaf*)</c> with no children of its own, and FDA's
        /// regional DTD declares nothing above Module 1.
        /// </summary>
        /// <remarks>
        /// Read from the ICH chain rather than the regional one because a
        /// Module 1 section still has an ICH ancestor: <c>m1-…</c> is where the
        /// chain stops, and its sub-sections contribute nothing below it.
        /// </remarks>
        public bool IsRegional =>
            IchElements is [ModuleOneElement, ..];
    }
}
