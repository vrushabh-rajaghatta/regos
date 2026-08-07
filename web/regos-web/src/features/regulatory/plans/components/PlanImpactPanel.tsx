import { usePlanImpact } from "../hooks/usePlanImpact";

/**
 * What today's facts imply, if nothing changes.
 *
 * **The labels are load-bearing.** *Planned finish* is what the plan says and
 * always will; *projected finish* is an analysis computed on request and thrown
 * away. Showing a single "finish date" would let a reader assume the projection
 * had become the plan — which is precisely what it must never do.
 *
 * It also never suggests what to move. The moment it did, it would have stopped
 * being analysis and started being a scheduler.
 */
export function PlanImpactPanel({ planId }: { planId: string }) {
  const { data, isPending, isError } = usePlanImpact(planId);

  if (isPending || isError || !data) return null;

  const onTrack = data.slipDays === 0 && data.lateSteps.length === 0;

  return (
    <section className="mt-8" data-testid="plan-impact">
      <h2 className="text-sm font-medium">Impact</h2>

      <p className="mt-1 max-w-prose text-sm text-muted-foreground">
        What the plan now implies, given what has been recorded. This is an
        analysis — it changes nothing, and the plan keeps the dates it was
        scheduled with.
      </p>

      <div className="mt-3 grid gap-4 rounded-lg border p-4 sm:grid-cols-3">
        <div>
          <div className="text-xs text-muted-foreground">Planned finish</div>
          <div className="mt-0.5 font-medium">
            {data.plannedFinishOn ?? "—"}
          </div>
        </div>

        <div>
          <div className="text-xs text-muted-foreground">
            Projected finish{" "}
            <span className="italic">(if nothing changes)</span>
          </div>
          <div className="mt-0.5 font-medium" data-testid="projected-finish">
            {data.projectedFinishOn ?? "—"}
          </div>
        </div>

        <div>
          <div className="text-xs text-muted-foreground">Current impact</div>
          <div
            className={`mt-0.5 font-medium ${
              data.slipDays > 0 ? "text-destructive" : ""
            }`}
            data-testid="slip-days"
          >
            {data.slipDays > 0 ? `+${data.slipDays} days` : "None"}
          </div>
        </div>
      </div>

      {onTrack && (
        <p className="mt-3 text-sm text-muted-foreground">
          Nothing is overdue and the finish date has not moved.
        </p>
      )}

      {data.lateSteps.length > 0 && (
        <div className="mt-4 space-y-3" data-testid="late-steps">
          {data.lateSteps.map((step) => (
            <div key={step.stepId} className="rounded-lg border p-4">
              <div className="flex flex-wrap items-baseline justify-between gap-2">
                <div>
                  <span className="font-medium">{step.name}</span>

                  <span className="ml-2 font-mono text-xs text-muted-foreground">
                    {step.code}
                  </span>
                </div>

                <span className="text-sm font-medium text-destructive">
                  {step.daysLate} days late
                </span>
              </div>

              <div className="mt-1 text-xs text-muted-foreground">
                due {step.plannedEndOn} · now expected {step.projectedEndOn}
              </div>

              {step.affected.length > 0 && (
                <div className="mt-3">
                  <div className="text-xs text-muted-foreground">
                    Affects {step.affected.length} downstream{" "}
                    {step.affected.length === 1 ? "step" : "steps"}
                  </div>

                  <div className="mt-1 flex flex-wrap gap-1">
                    {step.affected.map((affected) => (
                      <span
                        key={affected.stepId}
                        title={
                          affected.isActionable
                            ? "Still open"
                            : `Already ${affected.status.toLowerCase()} — affected, but nothing to do`
                        }
                        className={`rounded px-1.5 py-0.5 font-mono text-xs ${
                          affected.isActionable
                            ? "bg-muted"
                            : "bg-muted/40 text-muted-foreground line-through"
                        }`}
                      >
                        {affected.code}
                      </span>
                    ))}
                  </div>
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </section>
  );
}
