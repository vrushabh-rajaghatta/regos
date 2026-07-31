using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.ReferenceData.Domain.Blueprint;
using RegOS.SharedKernel.Exceptions;

using SubmissionAggregate = RegOS.Submission.Domain.Submission.Submission;

namespace RegOS.Submission.Application.Placement;

/// <summary>
/// The one rule the <see cref="SubmissionAggregate"/> cannot enforce about
/// placement: a document may only be placed into a section of the blueprint
/// version the submission is actually bound to.
/// </summary>
/// <remarks>
/// Sections are Reference Data, so the aggregate cannot see them — and reaching
/// across that boundary from inside it would be worse than the application layer
/// owning the rule. Shared by attach-with-placement and place-existing, because
/// a rule enforced in one path and not the other is not a rule.
/// <para>
/// Without it, a document could be placed into a section belonging to some other
/// template version and produce a dossier organised against a standard the
/// submission is not judged by — invisible until someone opened the content plan
/// and found a section that was not in their blueprint.
/// </para>
/// </remarks>
internal static class SectionPlacementPolicy
{
    public static async Task EnsureSectionIsInBoundBlueprintAsync(
        RegOSDbContext dbContext,
        SubmissionAggregate submission,
        TemplateSectionId sectionId,
        CancellationToken cancellationToken)
    {
        if (submission.BoundTemplateVersionId is not { } versionId)
            throw new BusinessRuleViolationException(
                SubmissionRuleErrors.SubmissionHasNoBlueprintToPlaceInto);

        // Reached through the aggregate root rather than Set<TemplateSection>()
        // so the tenant query filter on RegulatoryTemplates applies (ADR-031) —
        // a tenant-owned template's sections must not be placeable by anyone
        // else. This also answers "does it exist?" and "is it in the bound
        // version?" as one question, which is the only distinction that matters
        // here and avoids disclosing whether a section exists elsewhere.
        var isInBoundVersion = await dbContext.RegulatoryTemplates
            .AsNoTracking()
            .SelectMany(t => t.Versions)
            .Where(v => v.Id == versionId)
            .SelectMany(v => v.Sections)
            .AnyAsync(s => s.Id == sectionId, cancellationToken);

        if (!isInBoundVersion)
            throw new BusinessRuleViolationException(
                SubmissionRuleErrors.TemplateSectionNotInBoundBlueprint);
    }
}
