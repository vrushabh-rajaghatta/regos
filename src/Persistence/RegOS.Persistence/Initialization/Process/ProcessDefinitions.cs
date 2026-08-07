using RegOS.Persistence.Initialization.ReferenceData;
using RegOS.Process.Domain.Aggregates.ProcessDefinitions;
using RegOS.ReferenceData.Domain.ApplicationType;
using RegOS.ReferenceData.Domain.Geography.Country;
using RegOS.ReferenceData.Domain.Regulatory.Authority;

namespace RegOS.Persistence.Initialization.Process;

/// <summary>
/// The playbooks RegOS ships. One today: opening an IND with FDA.
/// </summary>
/// <remarks>
/// <b>Platform-owned</b> — every definition here has a null <c>TenantId</c>, so
/// every tenant can instantiate it and none can edit it. A tenant's own house
/// process sits beside these (ADR-031's shared-plus-extensible shape); authoring
/// it is EPIC-012's.
/// <para>
/// <b>One vertical, deliberately.</b> RegOS is proving US·FDA·IND end to end, and
/// a second half-remembered playbook for a market nobody has filed in would be a
/// worse artefact than none — the seed would look authoritative and be a guess.
/// </para>
/// </remarks>
internal static class ProcessDefinitions
{
    public static IReadOnlyList<ProcessDefinition> Data =>
    [
        BuildFdaIndInitial()
    ];

    // ── United States · FDA · IND — initial filing ───────────────────────────
    //
    // Twelve steps, and the shape is the point rather than the exact durations:
    // two long packages run from the anchor in parallel with the pre-IND meeting
    // track, three strands converge on compilation, and the 30-day safety review
    // is the tail nobody can compress.
    private static ProcessDefinition BuildFdaIndInitial()
    {
        var definition = ProcessDefinition.Create(
            new ProcessDefinitionId(ProcessDefinitionIds.FdaIndInitial),
            "US-FDA-IND-INITIAL",
            "US FDA IND — initial filing",
            new CountryId(GeographyAndRegulatoryIds.UnitedStates),
            new AuthorityId(GeographyAndRegulatoryIds.FDA),
            new ApplicationTypeId(ApplicationTypeIds.FdaInd),
            DateTime.UtcNow,
            "From the pre-IND meeting request through to the end of FDA's "
            + "30-day safety review.");

        var v1 = definition.StartDraftVersion();

        // The pre-IND track. A meeting request is the first thing filed, and
        // everything downstream of it waits on FDA's calendar rather than ours.
        var request = definition.AddStep(
            "PRE-IND-REQ",
            "Submit pre-IND meeting request",
            "Form FDA 3620 with the proposed questions and meeting format.",
            order: 10,
            durationDays: 5);

        var package = definition.AddStep(
            "PRE-IND-PKG",
            "Prepare and submit the pre-IND briefing package",
            "Due no later than 30 days before the meeting.",
            order: 20,
            offsetDays: 30,
            durationDays: 30);

        var meeting = definition.AddStep(
            "PRE-IND-MTG",
            "Pre-IND meeting with FDA",
            order: 30,
            offsetDays: 30,
            durationDays: 1);

        var minutes = definition.AddStep(
            "PRE-IND-MIN",
            "Receive FDA's official meeting minutes",
            "FDA issues minutes within 30 days of the meeting.",
            order: 40,
            offsetDays: 30,
            durationDays: 1);

        // The two long packages. Both start at the anchor — they are the reason
        // an IND takes as long as it does, and neither waits for the meeting.
        var nonclinical = definition.AddStep(
            "NONCLIN",
            "Complete the nonclinical package",
            "CTD Module 4 — pharmacology, pharmacokinetics and toxicology.",
            order: 50,
            durationDays: 120);

        var cmc = definition.AddStep(
            "CMC",
            "Complete the CMC package",
            "CTD Module 3 — drug substance and drug product.",
            order: 60,
            durationDays: 150);

        var protocol = definition.AddStep(
            "PROTOCOL",
            "Finalise the clinical protocol",
            "Written against what FDA actually said, not what we asked.",
            order: 70,
            durationDays: 45);

        var brochure = definition.AddStep(
            "IB",
            "Assemble the Investigator's Brochure",
            order: 80,
            durationDays: 21);

        var forms = definition.AddStep(
            "FORMS",
            "Complete FDA forms 1571, 1572 and 3674",
            order: 90,
            durationDays: 10);

        var compile = definition.AddStep(
            "COMPILE",
            "Compile and QC the eCTD sequence",
            "Where the three strands meet — nothing is submitted until they do.",
            order: 100,
            durationDays: 15);

        var submit = definition.AddStep(
            "SUBMIT",
            "Transmit the IND to FDA",
            "Via the Electronic Submissions Gateway.",
            order: 110,
            durationDays: 1);

        var safetyReview = definition.AddStep(
            "SAFETY-30",
            "FDA 30-day safety review",
            "The study may not begin until this elapses without a clinical hold.",
            order: 120,
            durationDays: 30);

        // The graph. Each edge says what a step waits for, never what waits for
        // it — successors are the reverse of these and are derived on read.
        definition.AddStepPredecessor(package.Id, request.Id);
        definition.AddStepPredecessor(meeting.Id, package.Id);
        definition.AddStepPredecessor(minutes.Id, meeting.Id);

        definition.AddStepPredecessor(protocol.Id, minutes.Id);
        definition.AddStepPredecessor(brochure.Id, nonclinical.Id);
        definition.AddStepPredecessor(forms.Id, protocol.Id);

        definition.AddStepPredecessor(compile.Id, cmc.Id);
        definition.AddStepPredecessor(compile.Id, brochure.Id);
        definition.AddStepPredecessor(compile.Id, forms.Id);

        definition.AddStepPredecessor(submit.Id, compile.Id);
        definition.AddStepPredecessor(safetyReview.Id, submit.Id);

        // Publishing certifies the graph is schedulable (ADR-065 I4). If an edit
        // above ever closes a loop, this line throws at startup rather than
        // shipping a playbook no plan could be derived from.
        definition.PublishVersion(v1.Id, new DateOnly(2026, 8, 6), DateTime.UtcNow);

        return definition;
    }
}
