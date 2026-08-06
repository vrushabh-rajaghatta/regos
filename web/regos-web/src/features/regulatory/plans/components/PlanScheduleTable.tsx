import type { PlannedStep } from "../types/ProcessPlan";

/**
 * The derived schedule.
 *
 * Dates are **inclusive** — a five-day step starting on the 1st ends on the 5th.
 * They were worked out once, when the plan was created, and nothing recalculates
 * them: moving one step moves nothing else, by design.
 */
export function PlanScheduleTable({ steps }: { steps: PlannedStep[] }) {
  return (
    <div className="overflow-x-auto rounded-lg border">
      <table className="w-full text-sm" data-testid="plan-schedule">
        <thead className="bg-muted/50 text-left">
          <tr>
            <th className="p-3 font-medium">Step</th>
            <th className="p-3 font-medium">Waits for</th>
            <th className="p-3 font-medium whitespace-nowrap">Starts</th>
            <th className="p-3 font-medium whitespace-nowrap">Ends</th>
          </tr>
        </thead>

        <tbody>
          {steps.map((step) => (
            <tr key={step.id} data-testid="plan-step-row" className="border-t align-top">
              <td className="p-3">
                <div className="font-medium">{step.name}</div>

                <div className="mt-0.5 font-mono text-xs text-muted-foreground">
                  {step.code}
                </div>
              </td>

              <td className="p-3">
                {step.predecessors.length === 0 ? (
                  <span className="text-muted-foreground">
                    the plan&rsquo;s start
                  </span>
                ) : (
                  <div className="flex flex-wrap gap-1">
                    {step.predecessors.map((code) => (
                      <span
                        key={code}
                        className="rounded bg-muted px-1.5 py-0.5 font-mono text-xs"
                      >
                        {code}
                      </span>
                    ))}
                  </div>
                )}
              </td>

              <td className="p-3 whitespace-nowrap">{step.plannedStartOn}</td>
              <td className="p-3 whitespace-nowrap">{step.plannedEndOn}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
