import { Link, useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { PageHeader } from "@/shared/components/PageHeader";

import { ObjectiveStatusBadge } from "../components/ObjectiveStatusBadge";
import { useChangeObjectiveStatus } from "../hooks/useChangeObjectiveStatus";
import { useObjective } from "../hooks/useObjective";

/**
 * The transitions a user may make from each state. Terminal states offer none —
 * an achieved or abandoned objective is history, and a new one is stated rather
 * than an old one reopened.
 */
const NEXT: Record<string, string[]> = {
  Proposed: ["Active", "Abandoned"],
  Active: ["Achieved", "Abandoned"],
  Achieved: [],
  Abandoned: [],
};

export function ObjectiveDetailPage() {
  const { objectiveId } = useParams();

  const { data, isPending, isError, refetch } = useObjective(objectiveId);
  const changeStatus = useChangeObjectiveStatus(objectiveId ?? "");

  return (
    <>
      <Link
        to="/regulatory/objectives"
        className="text-sm text-muted-foreground hover:underline"
      >
        ← Objectives
      </Link>

      <div className="mt-2">
        {isPending && (
          <div className="rounded-lg border p-8 text-center text-muted-foreground">
            Loading objective...
          </div>
        )}

        {!isPending && isError && (
          <div className="rounded-lg border border-destructive/40 p-8 text-center">
            <p className="text-sm text-destructive" role="alert">
              Could not load this objective.
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
              description={`${data.productName} · ${data.countryName}`}
            />

            <div className="mt-4 flex flex-wrap items-center gap-2">
              <ObjectiveStatusBadge status={data.status} />

              <span className="text-xs text-muted-foreground">
                Stated {data.statedOn}
              </span>

              {data.targetCompletionOn && (
                <span className="text-xs text-muted-foreground">
                  · Target {data.targetCompletionOn}
                </span>
              )}

              {data.achievedOn && (
                <span className="text-xs text-muted-foreground">
                  · Achieved {data.achievedOn}
                </span>
              )}
            </div>

            {data.rationale && (
              <section className="mt-8">
                <h2 className="text-sm font-medium">Rationale</h2>

                <p className="mt-2 max-w-prose text-sm text-muted-foreground">
                  {data.rationale}
                </p>
              </section>
            )}

            <section className="mt-8">
              <h2 className="text-sm font-medium">Market record</h2>

              <p className="mt-2 max-w-prose text-sm text-muted-foreground">
                {data.medicinalProductId
                  ? "The market record that fulfils this objective exists and is linked."
                  : "No market record yet. An objective is stated before the regulatory record for its market is created, so this is the normal early state."}
              </p>
            </section>

            <section className="mt-8">
              <div className="flex flex-wrap items-center gap-2">
                <h2 className="text-sm font-medium">Status</h2>

                {(NEXT[data.status] ?? []).map((target) => (
                  <Button
                    key={target}
                    size="sm"
                    variant="outline"
                    data-testid="objective-status-action"
                    disabled={changeStatus.isPending}
                    onClick={() =>
                      changeStatus.mutate({
                        status: target,
                        occurredOn: new Date().toISOString().slice(0, 10),
                        note: null,
                      })
                    }
                  >
                    Mark {target.toLowerCase()}
                  </Button>
                ))}
              </div>

              {changeStatus.isError && (
                <p className="mt-2 text-sm text-destructive" role="alert">
                  {changeStatus.error.message}
                </p>
              )}

              <div className="mt-4 overflow-x-auto rounded-lg border">
                <table className="w-full text-sm" data-testid="objective-history">
                  <thead className="bg-muted/50 text-left">
                    <tr>
                      <th className="p-3 font-medium">Status</th>
                      <th className="p-3 font-medium">On</th>
                      <th className="p-3 font-medium">Recorded</th>
                      <th className="p-3 font-medium">Note</th>
                    </tr>
                  </thead>

                  <tbody>
                    {data.history.map((entry) => (
                      <tr
                        key={`${entry.status}-${entry.recordedOnUtc}`}
                        className="border-t"
                      >
                        <td className="p-3">{entry.status}</td>
                        <td className="p-3 whitespace-nowrap">
                          {entry.occurredOn}
                        </td>
                        <td className="p-3 whitespace-nowrap text-muted-foreground">
                          {entry.recordedOnUtc.slice(0, 10)}
                        </td>
                        <td className="p-3 text-muted-foreground">
                          {entry.note ?? "—"}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </section>
          </>
        )}
      </div>
    </>
  );
}
