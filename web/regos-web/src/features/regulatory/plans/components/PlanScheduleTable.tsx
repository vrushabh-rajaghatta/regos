import { Button } from "@/components/ui/button";

import { useChangeStepStatus } from "../hooks/useChangeStepStatus";
import type { PlannedStep } from "../types/ProcessPlan";

interface PlanScheduleTableProps {
  planId: string;
  steps: PlannedStep[];
  /** Work can only be recorded against an active plan. */
  canRecord: boolean;
}

const today = () => new Date().toISOString().slice(0, 10);

/**
 * The schedule, and where work is recorded against it.
 *
 * Planned dates were worked out once and nothing recalculates them. Execution is
 * separate: **a step becomes complete because a person says so** — no linked
 * submission, meeting or finished predecessor ever moves it.
 *
 * Attached records are shown because they are discoverable from here, not
 * because they mean anything about the step's state. **A step with nothing
 * attached is not incomplete** — it may simply not have been annotated.
 */
export function PlanScheduleTable({
  planId,
  steps,
  canRecord,
}: PlanScheduleTableProps) {
  const change = useChangeStepStatus(planId);

  function record(
    stepId: string,
    status: "InProgress" | "Complete" | "Skipped",
  ) {
    // The one place friction is deliberate: a skipped step with no reason is an
    // unexplained gap in a regulatory record a year later.
    const note =
      status === "Skipped"
        ? window.prompt("Why is this step being skipped?")
        : null;

    if (status === "Skipped" && !note?.trim()) return;

    change.mutate({ stepId, status, occurredOn: today(), note });
  }

  return (
    <>
      <div className="overflow-x-auto rounded-lg border">
        <table className="w-full text-sm" data-testid="plan-schedule">
          <thead className="bg-muted/50 text-left">
            <tr>
              <th className="p-3 font-medium">Step</th>
              <th className="p-3 font-medium">Waits for</th>
              <th className="p-3 font-medium whitespace-nowrap">Planned</th>
              <th className="p-3 font-medium whitespace-nowrap">Actual</th>
              <th className="p-3 font-medium">Status</th>
            </tr>
          </thead>

          <tbody>
            {steps.map((step) => (
              <tr
                key={step.id}
                data-testid="plan-step-row"
                className="border-t align-top"
              >
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

                <td className="p-3 whitespace-nowrap text-muted-foreground">
                  {step.plannedStartOn} to {step.plannedEndOn}

                  {step.attached.length > 0 && (
                    <div className="mt-2 space-y-0.5">
                      {step.attached.map((artefact) => (
                        <div
                          key={`${artefact.kind}-${artefact.id}`}
                          data-testid="step-attachment"
                          className="text-xs whitespace-normal"
                        >
                          <span className="text-muted-foreground">
                            {artefact.kind}:
                          </span>{" "}
                          {artefact.title}
                        </div>
                      ))}
                    </div>
                  )}
                </td>

                <td className="p-3 whitespace-nowrap text-muted-foreground">
                  {step.actualStartOn ?? "—"}
                  {step.actualEndOn ? ` to ${step.actualEndOn}` : ""}
                </td>

                <td className="p-3">
                  <div className="font-medium">{step.status}</div>

                  {canRecord && !step.isSettled && (
                    <div className="mt-2 flex flex-wrap gap-1">
                      {step.status === "NotStarted" && (
                        <Button
                          size="sm"
                          variant="outline"
                          data-testid="step-start"
                          onClick={() => record(step.id, "InProgress")}
                        >
                          Start
                        </Button>
                      )}

                      <Button
                        size="sm"
                        variant="outline"
                        data-testid="step-complete"
                        onClick={() => record(step.id, "Complete")}
                      >
                        Complete
                      </Button>

                      <Button
                        size="sm"
                        variant="ghost"
                        data-testid="step-skip"
                        onClick={() => record(step.id, "Skipped")}
                      >
                        Skip
                      </Button>
                    </div>
                  )}

                  {step.status === "Skipped" && (
                    <p className="mt-1 max-w-xs text-xs text-muted-foreground">
                      {step.history.find((x) => x.status === "Skipped")?.note}
                    </p>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {change.isError && (
        <p className="mt-2 text-sm text-destructive" role="alert">
          {change.error.message}
        </p>
      )}
    </>
  );
}
