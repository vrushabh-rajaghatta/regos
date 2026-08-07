namespace RegOS.Process.Application.Queries.ListNextSteps;

/// <summary>
/// <em>"What can I work on today?"</em> — across every active plan a tenant holds.
/// </summary>
/// <param name="AsOf">
/// The date lateness is judged against. <b>A parameter, never a clock read inside
/// the handler.</b>
/// <para>
/// I5 forbids a clock in the derivation of a <em>schedule</em>; it says nothing
/// about a read. But passing the date in anyway buys four things worth having in
/// regulated software: deterministic tests, replayable historical queries,
/// reproducible bug reports, and no hidden dependency on the server's clock.
/// The endpoint supplies today; the handler never asks.
/// </para>
/// </param>
public sealed record ListNextStepsQuery(DateOnly AsOf);
