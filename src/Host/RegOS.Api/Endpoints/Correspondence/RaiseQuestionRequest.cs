namespace RegOS.Api.Endpoints.Correspondence;

/// <param name="TargetResponseOn">
/// Our internal target, not the letter's regulatory deadline — that is the
/// correspondence's own <c>ResponseDueOn</c>. Two clocks, two words.
/// </param>
public sealed record RaiseQuestionRequest(
    string Number,
    string Text,
    DateOnly? TargetResponseOn = null);
