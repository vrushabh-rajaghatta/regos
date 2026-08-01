import { useState } from "react";
import { Link } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";

import { useDueWork } from "../hooks/useDueWork";
import { dueLabel, dueProximity } from "../utils/dueProximity";

/**
 * What do I need to work on today?
 *
 * The epic's headline read, and the first screen in RegOS that answers a
 * question about **work** rather than about **data**. Three obligations from
 * three aggregates in one list, because a person does not sort their morning by
 * aggregate type.
 *
 * A letter appears here only until its work has been decomposed: once it has
 * questions, the questions are the work and the letter is not. That rule lives
 * in the read model — nothing marks a letter as hidden.
 */
export function DueWorkPage() {
  const [mine, setMine] = useState(false);
  const [horizon, setHorizon] = useState("");

  const { data, isLoading, error } = useDueWork(mine, horizon || undefined);
  const rows = data ?? [];

  return (
    <Page>
      <PageHeader
        title="What's due"
        description="Everything still owed to a health authority, soonest first."
      />

      <div className="mb-4 flex flex-wrap items-center gap-3">
        <Button
          type="button"
          variant={mine ? "default" : "outline"}
          onClick={() => setMine(!mine)}
        >
          {mine ? "Showing mine" : "Show only mine"}
        </Button>

        <label className="text-sm" htmlFor="dueHorizon">
          Due on or before
          <input
            id="dueHorizon"
            type="date"
            className="ml-2 h-9 rounded-md border bg-transparent px-3 text-sm"
            value={horizon}
            onChange={(event) => setHorizon(event.target.value)}
          />
        </label>
      </div>

      {isLoading && <p className="text-muted-foreground">Loading...</p>}
      {error && <p className="text-destructive">Failed to load what is due.</p>}

      {!isLoading && !error && rows.length === 0 && (
        <div
          className="rounded-lg border border-dashed p-8 text-center"
          data-testid="due-work-empty"
        >
          <h3 className="text-lg font-semibold">Nothing is outstanding.</h3>
          <p className="mt-1 text-sm text-muted-foreground">
            No letters awaiting review, no open questions, no commitments owed.
          </p>
        </div>
      )}

      {rows.length > 0 && (
        <div className="overflow-x-auto rounded-lg border">
          <table className="w-full text-sm">
            <thead className="bg-muted/50 text-left">
              <tr>
                <th className="px-4 py-2 font-medium">Work</th>
                <th className="px-4 py-2 font-medium">What</th>
                <th className="px-4 py-2 font-medium">Authority</th>
                <th className="px-4 py-2 font-medium">Status</th>
                <th className="px-4 py-2 font-medium">Due</th>
              </tr>
            </thead>

            <tbody>
              {rows.map((row) => {
                const proximity = dueProximity(row.dueOn);

                return (
                  <tr
                    key={`${row.kind}-${row.id}`}
                    className="border-t"
                    data-testid="due-work-row"
                  >
                    <td className="px-4 py-2">{row.kind}</td>
                    <td className="px-4 py-2">
                      {row.correspondenceId ? (
                        <Link
                          className="underline-offset-4 hover:underline"
                          to={`/regulatory/correspondence/${row.correspondenceId}`}
                        >
                          {row.title}
                        </Link>
                      ) : (
                        row.title
                      )}
                    </td>
                    <td className="px-4 py-2">{row.authorityName}</td>
                    <td className="px-4 py-2">{row.status}</td>
                    <td
                      className={
                        proximity === "overdue" || proximity === "today"
                          ? "px-4 py-2 font-medium text-destructive"
                          : proximity === "soon"
                            ? "px-4 py-2 font-medium text-amber-600"
                            : "px-4 py-2 text-muted-foreground"
                      }
                    >
                      {dueLabel(row.dueOn)}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}
    </Page>
  );
}
