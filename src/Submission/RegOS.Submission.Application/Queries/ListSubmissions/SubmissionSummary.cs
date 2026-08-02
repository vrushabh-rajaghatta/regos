namespace RegOS.Submission.Application.Queries.ListSubmissions;

public sealed record SubmissionSummary(
    Guid Id,
    string Title,
    string Status,
    string SubmissionTypeName,
    DateTime CreatedOn,
    // What this was filed as — "Sequence 0003" on screen. Null while a draft,
    // because a draft has not been transmitted and has no number (ADR-044).
    int? SequenceNumber);
