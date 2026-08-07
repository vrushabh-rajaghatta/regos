import { Link, useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { PageHeader } from "@/shared/components/PageHeader";

import { PlanImpactPanel } from "../components/PlanImpactPanel";
import { PlanScheduleTable } from "../components/PlanScheduleTable";
import { useChangePlanStatus } from "../hooks/useChangePlanStatus";
import { usePlan } from "../hooks/usePlan";

export function PlanDetailPage() {
  const { planId } = useParams();

  const { data, isPending, isError, refetch } = usePlan(planId);
  const changeStatus = useChangePlanStatus(planId ?? "");

  const next: Record<string, ("Active" | "Completed" | "Cancelled")[]> = {
    Draft: ["Active", "Cancelled"],
    Active: ["Completed", "Cancelled"],
    Completed: [],
    Cancelled: [],
  };

  return (
    <>
      {data && (
        <Link
          to={`/regulatory/objectives/${data.processObjectiveId}`}
          className="text-sm text-muted-foreground hover:underline"
        >
          ← {data.objectiveName}
        </Link>
      )}

      <div className="mt-2">
        {isPending && (
          <div className="rounded-lg border p-8 text-center text-muted-foreground">
            Loading plan...
          </div>
        )}

        {!isPending && isError && (
          <div className="rounded-lg border border-destructive/40 p-8 text-center">
            <p className="text-sm text-destructive" role="alert">
              Could not load this plan.
            </p>

            <Button variant="outline" className="mt-3" onClick={() => refetch()}>
              Retry
            </Button>
          </div>
        )}

        {!isPending && !isError && data && (
          <>
            <PageHeader
              title={data.name}
              description={`${data.definitionName} v${data.definitionVersionNumber} · scheduled from ${data.anchorDate}`}
            />

            {data.definitionVersionIsSuperseded && (
              <div
                className="mt-4 rounded-lg border p-4 text-sm"
                data-testid="plan-superseded-notice"
              >
                <p className="font-medium">
                  A newer version of this playbook has been published.
                </p>

                <p className="mt-1 text-muted-foreground">
                  This plan stays on v{data.definitionVersionNumber} and nothing
                  about its dates has changed. A plan records what was scheduled,
                  from which version, on which day — so a later playbook can never
                  move a milestone that has already been agreed.
                </p>
              </div>
            )}

            <div className="mt-4 flex flex-wrap items-center gap-2">
              {(next[data.status] ?? []).map((target) => (
                <Button
                  key={target}
                  size="sm"
                  variant="outline"
                  data-testid="plan-status-action"
                  disabled={changeStatus.isPending}
                  onClick={() =>
                    changeStatus.mutate({
                      status: target,
                      occurredOn: new Date().toISOString().slice(0, 10),
                      note: null,
                    })
                  }
                >
                  {target === "Active" ? "Activate" : `Mark ${target.toLowerCase()}`}
                </Button>
              ))}
            </div>

            {changeStatus.isError && (
              <p className="mt-2 text-sm text-destructive" role="alert">
                {changeStatus.error.message}
              </p>
            )}

            <div className="mt-4 flex flex-wrap items-center gap-3 text-xs text-muted-foreground">
              <span>{data.status}</span>
              <span>· {data.steps.length} steps</span>

              {data.plannedStartOn && data.plannedEndOn && (
                <span>
                  · {data.plannedStartOn} to {data.plannedEndOn}
                </span>
              )}
            </div>

            {data.status === "Active" && <PlanImpactPanel planId={data.id} />}

            <section className="mt-8">
              <h2 className="text-sm font-medium">Schedule</h2>

              <p className="mt-1 max-w-prose text-sm text-muted-foreground">
                Planned dates were worked out once and nothing recalculates
                them. A step is complete because someone says so — a linked
                submission or a finished predecessor may suggest it, never
                perform it.
              </p>

              <div className="mt-3">
                <PlanScheduleTable
                  planId={data.id}
                  steps={data.steps}
                  canRecord={data.status === "Active"}
                />
              </div>
            </section>
          </>
        )}
      </div>
    </>
  );
}
