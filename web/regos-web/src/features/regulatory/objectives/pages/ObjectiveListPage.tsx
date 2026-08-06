import { useState } from "react";
import { Link } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { PageHeader } from "@/shared/components/PageHeader";

import { ObjectiveStatusBadge } from "../components/ObjectiveStatusBadge";
import { StateObjectiveDialog } from "../components/StateObjectiveDialog";
import { useObjectives } from "../hooks/useObjectives";

export function ObjectiveListPage() {
  const [stating, setStating] = useState(false);
  const [includeClosed, setIncludeClosed] = useState(false);

  const { data, isPending, isError, refetch } = useObjectives(includeClosed);

  return (
    <>
      <PageHeader
        title="Objectives"
        description="What we are trying to achieve, and where. An objective is the goal; plans are attempts at it."
      />

      <div className="mt-4 flex flex-wrap items-center gap-2">
        <Button onClick={() => setStating(true)}>State objective</Button>

        <Button
          variant="outline"
          onClick={() => setIncludeClosed((current) => !current)}
        >
          {includeClosed ? "Hide closed" : "Show achieved and abandoned"}
        </Button>
      </div>

      <div className="mt-6">
        {isPending && (
          <div className="rounded-lg border p-8 text-center text-muted-foreground">
            Loading objectives...
          </div>
        )}

        {!isPending && isError && (
          <div className="rounded-lg border border-destructive/40 p-8 text-center">
            <p className="text-sm text-destructive" role="alert">
              Could not load objectives. Check that the API is running.
            </p>

            <Button variant="outline" className="mt-3" onClick={() => refetch()}>
              Retry
            </Button>
          </div>
        )}

        {!isPending && !isError && data && data.length === 0 && (
          <div className="rounded-lg border border-dashed p-8 text-center text-muted-foreground">
            Nothing stated yet. An objective is where a filing starts.
          </div>
        )}

        {!isPending && !isError && data && data.length > 0 && (
          <div className="space-y-3" data-testid="objective-list">
            {data.map((objective) => (
              <Link
                key={objective.id}
                to={objective.id}
                data-testid="objective-row"
                className="block rounded-lg border p-4 transition-colors hover:bg-muted"
              >
                <div className="flex items-start justify-between gap-4">
                  <div>
                    <div className="font-medium">{objective.name}</div>

                    <div className="mt-1 text-sm text-muted-foreground">
                      {objective.productName} · {objective.countryName}
                    </div>

                    {!objective.hasMarketRecord && (
                      <p className="mt-2 text-xs text-muted-foreground">
                        No market record yet — normal for an objective this early.
                      </p>
                    )}
                  </div>

                  <div className="flex shrink-0 flex-col items-end gap-2">
                    <ObjectiveStatusBadge status={objective.status} />

                    {objective.targetCompletionOn && (
                      <span className="text-xs text-muted-foreground">
                        Target {objective.targetCompletionOn}
                      </span>
                    )}
                  </div>
                </div>
              </Link>
            ))}
          </div>
        )}
      </div>

      <StateObjectiveDialog open={stating} onOpenChange={setStating} />
    </>
  );
}
