using RegOS.Labeling.Domain.Aggregates.LocalLabels;

namespace RegOS.Labeling.Application.Commands.PublishLocalLabelRevision;

/// <param name="ApprovedOn">
/// The day the authority approved it. Required — a label in force that no
/// authority approved is a false statement about a regulated document.
/// </param>
/// <param name="EffectiveFrom">
/// The day it takes effect in this market. A different fact: <em>approved 12
/// May, effective 1 June</em> and <em>approved 12 May, effective
/// immediately</em> both occur.
/// </param>
public sealed record PublishLocalLabelRevisionCommand(
    LocalLabelId LocalLabelId,
    LocalLabelRevisionId RevisionId,
    DateOnly ApprovedOn,
    DateOnly EffectiveFrom);
