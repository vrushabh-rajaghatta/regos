import { Link } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { PageHeader } from "@/shared/components/PageHeader";

import { useNextSteps } from "../hooks/useNextSteps";

/**
 * What a team can work on today, across every active plan.
 *
 * **This is not the due-work view, and deliberately shares nothing with it.**
 * *Due* is an obligation a regulator is waiting on — missing one has compliance
 * consequence. *Late* is our own plan slipping — missing one affects
 * forecasting. Two different facts, two different lists, and a future "my work"
 * screen composes both rather than replacing either.
 */
export function PlanBoardPage() {
  const { data, isPending, isError, refetch } = useNextSteps();

  const late = data?.filter((step) => step.daysLate !== null) ?? [];
  const ready = data?.filter((step) => step.daysLate === null && step.isReady) ?? [];
  const blocked = data?.filter((step) => step.daysLate === null && !step.isReady) ?? [];

  return (
    <>
      <PageHeader
        title="Plan board"
        description="What your plans say you should be working on. Separate from due work, which is what a regulator is waiting for."
      />

      <div className="mt-6">
        {isPending && (
          <div className="rounded-lg border p-8 text-center text-muted-foreground">
            Loading the board...
          </div>
        )}

        {!isPending && isError && (
          <div className="rounded-lg border border-destructive/40 p-8 text-center">
            <p className="text-sm text-destructive" role="alert">
              Could not load the plan board.
            </p>

            <Button variant="outline" className="mt-3" onClick={() => refetch()}>
              Retry
            </Button>
          </div>
        )}

        {!isPending && !isError && data && data.length === 0 && (
          <div className="rounded-lg border border-dashed p-8 text-center text-muted-foreground">
            Nothing outstanding. Only active plans appear here — a draft has not
            been committed to yet.
          </div>
        )}

        {!isPending && !isError && data && data.length > 0 && (
          <div className="space-y-8">
            <Section
              title="Late"
              hint="Past its planned end date."
              steps={late}
              testId="board-late"
            />

            <Section
              title="Ready"
              hint="Everything it waits for is settled. Nothing has started it — that is someone's decision."
              steps={ready}
              testId="board-ready"
            />

            <Section
              title="Waiting"
              hint="Blocked on something earlier in the plan."
              steps={blocked}
              testId="board-waiting"
            />
          </div>
        )}
      </div>
    </>
  );
}

function Section({
  title,
  hint,
  steps,
  testId,
}: {
  title: string;
  hint: string;
  steps: ReturnType<typeof useNextSteps>["data"] extends (infer T)[] | undefined
    ? T[]
    : never;
  testId: string;
}) {
  if (steps.length === 0) return null;

  return (
    <section data-testid={testId}>
      <h2 className="text-sm font-medium">
        {title}
        <span className="ml-2 text-xs font-normal text-muted-foreground">
          {steps.length}
        </span>
      </h2>

      <p className="mt-1 max-w-prose text-xs text-muted-foreground">{hint}</p>

      <div className="mt-3 space-y-2">
        {steps.map((step) => (
          <Link
            key={step.stepId}
            to={`/regulatory/plans/${step.planId}`}
            data-testid="board-row"
            className="block rounded-lg border p-3 transition-colors hover:bg-muted"
          >
            <div className="flex items-start justify-between gap-4">
              <div>
                <div className="font-medium">{step.name}</div>

                <div className="mt-0.5 text-xs text-muted-foreground">
                  {step.objectiveName} · {step.countryCode} · {step.planName}
                </div>

                {step.waitingOn.length > 0 && (
                  <div className="mt-1 text-xs text-muted-foreground">
                    waiting on {step.waitingOn.join(", ")}
                  </div>
                )}
              </div>

              <div className="shrink-0 text-right text-xs text-muted-foreground">
                <div>{step.status}</div>
                <div>due {step.plannedEndOn}</div>

                {step.daysLate !== null && (
                  <div className="font-medium text-destructive">
                    {step.daysLate} days late
                  </div>
                )}
              </div>
            </div>
          </Link>
        ))}
      </div>
    </section>
  );
}
