using RegOS.SharedKernel.Exceptions;

namespace RegOS.Interaction.Domain.Correspondence;

/// <summary>
/// A question the authority raised inside a letter, and our answer to it.
/// </summary>
/// <remarks>
/// <b>A child of the correspondence, not a root.</b> A question has no meaning
/// without the letter it arrived in, and no user question reaches one without
/// going through the letter first. The pressure to promote it came from the
/// <em>"what's due"</em> view — and ADR-039 principle 7 removed that pressure
/// entirely, because a read model may project across boundaries freely. What
/// would justify promotion is behaviour, not reads, and answering a question
/// changes nothing on the letter.
/// <para>
/// <b><see cref="TargetResponseOn"/> is ours; the letter's
/// <c>ResponseDueOn</c> is theirs.</b> Two different clocks, and the "what's
/// due" view shows both at once — the same condition that made a shared word
/// unacceptable at the market tier. <em>Due</em> reads as an external
/// obligation, <em>target</em> reads as internal planning, so the two are
/// never one word.
/// </para>
/// <para>
/// <b>No owner yet, and its absence is a decision.</b> The owner of a question
/// is one of <em>our</em> people — a <c>UserId</c>, never a <c>Contact</c>,
/// which is a person at another company or at an authority. But no regulatory
/// context references <c>Platform.Domain</c> today; eight have kept that
/// boundary, and <c>Contact</c>'s own remarks warn against dragging Platform
/// identity into the regulatory domain. Making that edge is an architectural
/// decision, not a field, so it waits for S004 where <em>"who is answering
/// this?"</em> is actually read.
/// </para>
/// </remarks>
public sealed class HaQuestion
{
    public const int NumberMaxLength = 20;
    public const int TextMaxLength = 4000;
    public const int ResponseMaxLength = 8000;

    private readonly List<HaQuestionStatusEntry> _history = [];

    // EF materialisation only. The raising constructor below takes a date that
    // is not a mapped property — it seeds the first history entry — so EF
    // cannot bind by parameter name the way the other aggregates allow.
    private HaQuestion()
    {
    }

    // Only HaCorrespondence may raise a question.
    internal HaQuestion(
        HaQuestionId id,
        string number,
        string text,
        DateOnly? targetResponseOn,
        DateOnly raisedOn)
    {
        Id = id;
        Number = ValidatedNumber(number);
        Text = ValidatedText(text);
        TargetResponseOn = targetResponseOn;
        CurrentStatus = HaQuestionStatus.Open;

        _history.Add(new HaQuestionStatusEntry(
            HaQuestionStatusEntryId.New(),
            HaQuestionStatus.Open,
            raisedOn,
            DateTime.UtcNow,
            null));
    }

    public HaQuestionId Id { get; } = default!;

    /// <summary>
    /// As the letter numbers it — "1", "2a", "3.1". A string, because
    /// authorities do not agree that questions are integers.
    /// </summary>
    public string Number { get; private set; } = default!;

    public string Text { get; private set; } = default!;

    /// <summary>Our internal target. Not the letter's regulatory deadline.</summary>
    public DateOnly? TargetResponseOn { get; private set; }

    public string? ResponseText { get; private set; }

    public HaQuestionStatus CurrentStatus { get; private set; }

    public IReadOnlyList<HaQuestionStatusEntry> History
        => _history.AsReadOnly();

    /// <summary>
    /// When we replied — the <c>OccurredOn</c> of the first entry reaching
    /// <see cref="HaQuestionStatus.Responded"/>. Derived, never stored: a
    /// second copy of a fact the history already holds could disagree with it
    /// (ADR-037).
    /// </summary>
    public DateOnly? RespondedOn
        => _history
            .Where(x => x.Status == HaQuestionStatus.Responded)
            .Select(x => (DateOnly?)x.OccurredOn)
            .FirstOrDefault();

    /// <summary>Records our answer and moves the question to Responded.</summary>
    internal void Respond(string responseText, DateOnly occurredOn, string? note)
    {
        if (CurrentStatus == HaQuestionStatus.Resolved)
            throw new BusinessRuleViolationException(
                HaCorrespondenceErrors.QuestionAlreadyResolved);

        if (string.IsNullOrWhiteSpace(responseText))
            throw new DomainException(HaCorrespondenceErrors.ResponseRequired);

        var trimmed = responseText.Trim();

        if (trimmed.Length > ResponseMaxLength)
            throw new DomainException(HaCorrespondenceErrors.ResponseTooLong);

        ResponseText = trimmed;

        Append(HaQuestionStatus.Responded, occurredOn, note);
    }

    /// <summary>
    /// The authority accepted our answer. Terminal: a resolved question that
    /// reopens is a new question in a new letter, which is how authorities
    /// actually do it.
    /// </summary>
    internal void Resolve(DateOnly occurredOn, string? note)
    {
        if (CurrentStatus == HaQuestionStatus.Resolved)
            throw new BusinessRuleViolationException(
                HaCorrespondenceErrors.QuestionAlreadyResolved);

        Append(HaQuestionStatus.Resolved, occurredOn, note);
    }

    internal void Amend(string number, string text, DateOnly? targetResponseOn)
    {
        Number = ValidatedNumber(number);
        Text = ValidatedText(text);
        TargetResponseOn = targetResponseOn;
    }

    private void Append(HaQuestionStatus status, DateOnly occurredOn, string? note)
    {
        // The chronology rule. It lives here, on the aggregate's behaviour,
        // not on the entry — which is why an extraction of the entry alone
        // would take the cheap third of the duplication and leave this.
        if (occurredOn < _history[^1].OccurredOn)
            throw new DomainException(HaCorrespondenceErrors.QuestionHistoryOutOfOrder);

        if (note is { Length: > HaQuestionStatusEntry.NoteMaxLength })
            throw new DomainException(HaCorrespondenceErrors.NoteTooLong);

        _history.Add(new HaQuestionStatusEntry(
            HaQuestionStatusEntryId.New(),
            status,
            occurredOn,
            DateTime.UtcNow,
            string.IsNullOrWhiteSpace(note) ? null : note.Trim()));

        CurrentStatus = status;
    }

    private static string ValidatedNumber(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new DomainException(HaCorrespondenceErrors.QuestionNumberRequired);

        var trimmed = number.Trim();

        if (trimmed.Length > NumberMaxLength)
            throw new DomainException(HaCorrespondenceErrors.QuestionNumberTooLong);

        return trimmed;
    }

    private static string ValidatedText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new DomainException(HaCorrespondenceErrors.QuestionTextRequired);

        var trimmed = text.Trim();

        if (trimmed.Length > TextMaxLength)
            throw new DomainException(HaCorrespondenceErrors.QuestionTextTooLong);

        return trimmed;
    }
}
