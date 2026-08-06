namespace RegOS.Process.Application.Queries.GetPlanImpact;

/// <param name="AsOf">
/// What lateness and the projection are judged against. A parameter, never a
/// clock read inside the handler — so the same question always gets the same
/// answer, and a caller can ask what the picture looked like on any date.
/// </param>
public sealed record GetPlanImpactQuery(Guid PlanId, DateOnly AsOf);
