using Microsoft.EntityFrameworkCore;

using RegOS.Interaction.Domain.Commitments;
using RegOS.Interaction.Domain.Correspondence;
using RegOS.Persistence;

namespace RegOS.Interaction.Application.Queries.ListDueWork;

/// <summary>
/// The epic's headline read: what work still exists, across three aggregates.
/// </summary>
/// <remarks>
/// It answers <em>"what work remains?"</em> rather than <em>"what data
/// exists?"</em>, and the difference shows in the first rule below.
/// <para>
/// <b>Correspondence remains actionable until its work has been decomposed.</b>
/// A letter that has been broken into questions is no longer a task — the
/// questions are. Showing both would double-count one obligation. The rule is
/// derived, never stored: nothing marks a letter as "hidden", the read simply
/// asks whether it still represents work. Today decomposition means questions;
/// if it later also means commitments, the wording holds and the query changes.
/// </para>
/// <para>
/// Three reads composed in memory rather than one union in SQL. The volumes are
/// a team's open obligations, not a warehouse, and a readable projection is
/// worth more here than a clever one. Reads compose across aggregates freely
/// (ADR-039 principle 7).
/// </para>
/// </remarks>
public sealed class ListDueWorkHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListDueWorkHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<DueWorkItem>> HandleAsync(
        ListDueWorkQuery query,
        CancellationToken cancellationToken)
    {
        var authorities = await _dbContext.Authorities
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var items = new List<DueWorkItem>();

        // 1. Letters nobody has decomposed yet.
        var undecomposed = await _dbContext.HaCorrespondence
            .AsNoTracking()
            .Where(x => x.ResponseDueOn != null)
            .Where(x => !x.Questions.Any(q =>
                q.CurrentStatus != HaQuestionStatus.Resolved))
            .Select(x => new
            {
                x.Id,
                x.Subject,
                x.AuthorityId,
                x.ResponseDueOn,
                HasQuestions = x.Questions.Any()
            })
            .ToListAsync(cancellationToken);

        items.AddRange(undecomposed
            // A letter whose questions are all resolved is finished, not
            // undecomposed. Only one with no questions at all is still work.
            .Where(x => !x.HasQuestions)
            .Select(x => new DueWorkItem(
                "Correspondence",
                x.Id.Value,
                x.Id.Value,
                x.Subject,
                authorities.GetValueOrDefault(x.AuthorityId, "Unknown authority"),
                x.ResponseDueOn,
                null,
                "Awaiting review")));

        // 2. Open questions.
        var questions = await _dbContext.HaCorrespondence
            .AsNoTracking()
            .SelectMany(c => c.Questions
                .Where(q => q.CurrentStatus != HaQuestionStatus.Resolved)
                .Select(q => new
                {
                    CorrespondenceId = c.Id,
                    c.AuthorityId,
                    q.Id,
                    q.Number,
                    q.Text,
                    q.TargetResponseOn,
                    q.OwnerUserId,
                    q.CurrentStatus
                }))
            .ToListAsync(cancellationToken);

        items.AddRange(questions.Select(x => new DueWorkItem(
            "Question",
            x.Id.Value,
            x.CorrespondenceId.Value,
            $"{x.Number}. {x.Text}",
            authorities.GetValueOrDefault(x.AuthorityId, "Unknown authority"),
            x.TargetResponseOn,
            x.OwnerUserId?.Value,
            x.CurrentStatus.ToString())));

        // 3. Commitments still owed.
        var commitments = await _dbContext.Commitments
            .AsNoTracking()
            .Where(x => x.CurrentStatus != CommitmentStatus.Fulfilled
                && x.CurrentStatus != CommitmentStatus.Waived)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.AuthorityId,
                x.DueOn,
                x.OwnerUserId,
                x.CurrentStatus
            })
            .ToListAsync(cancellationToken);

        items.AddRange(commitments.Select(x => new DueWorkItem(
            "Commitment",
            x.Id.Value,
            null,
            x.Title,
            authorities.GetValueOrDefault(x.AuthorityId, "Unknown authority"),
            x.DueOn,
            x.OwnerUserId?.Value,
            x.CurrentStatus.ToString())));

        var filtered = items.AsEnumerable();

        if (query.OwnerUserId is { } owner)
            filtered = filtered.Where(x => x.OwnerUserId == owner.Value);

        if (query.DueOnOrBefore is { } horizon)
            filtered = filtered.Where(x => x.DueOn != null && x.DueOn <= horizon);

        // Undated work sorts last: it is real work with no clock on it, not
        // work that is infinitely far away.
        return filtered
            .OrderBy(x => x.DueOn is null)
            .ThenBy(x => x.DueOn)
            .ThenBy(x => x.Kind)
            .ThenBy(x => x.Id)
            .ToList();
    }
}
