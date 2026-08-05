import { useState } from "react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";

import { ManufacturingOperationDialog } from "./ManufacturingOperationDialog";
import { useCeaseManufacturingOperation } from "../hooks/useCeaseManufacturingOperation";
import { useManufacturingOperations } from "../hooks/useManufacturingOperations";

interface MarketManufacturingProps {
  medicinalProductId: string;
}

/**
 * **"Which sites make this product?"** — the question EPIC-010c S001 exists
 * for.
 *
 * **Closed periods are listed too, below the current ones.** A site that made
 * this product for four years made it, and hiding the row would make a filing
 * from 2023 unexplainable — the same call EPIC-010b made about a pack with no
 * licence, and EPIC-018 about a market with a withdrawn indication.
 *
 * **The site's name is read, never stored.** There is no manufacturer field
 * anywhere in RegOS (ADR-063 §3): a copied name is a second place for the truth
 * to live, and the first to go stale when a plant is renamed.
 */
export function MarketManufacturing({
  medicinalProductId,
}: MarketManufacturingProps) {
  const { data, isLoading, error } =
    useManufacturingOperations(medicinalProductId);

  const cease = useCeaseManufacturingOperation(medicinalProductId);

  const [recording, setRecording] = useState(false);
  const [closing, setClosing] = useState<string | undefined>();

  const operations = data ?? [];
  const current = operations.filter((operation) => operation.isCurrent);

  return (
    <section className="space-y-3" data-testid="market-manufacturing">
      <div className="flex items-start justify-between gap-2">
        <div>
          <h2 className="text-lg font-semibold">Where this is made</h2>
          <p className="text-sm text-muted-foreground">
            Every site that manufactures, packages, tests, releases or imports
            this market's product.
          </p>
        </div>

        <Button
          size="sm"
          variant="outline"
          onClick={() => setRecording(true)}
          data-testid="record-manufacturing"
        >
          Record an operation
        </Button>
      </div>

      {isLoading && (
        <p className="text-sm text-muted-foreground">Loading operations...</p>
      )}
      {error && (
        <p className="text-sm text-destructive">
          Failed to load where this product is made.
        </p>
      )}

      {cease.isError && (
        <p className="text-sm text-destructive" data-testid="cease-error">
          {(cease.error as Error).message}
        </p>
      )}

      {!isLoading && !error && operations.length === 0 && (
        <p
          className="rounded-lg border border-dashed p-6 text-center text-sm text-muted-foreground"
          data-testid="manufacturing-empty"
        >
          No operations recorded yet. Nothing here says a site is unapproved —
          only that nobody has said where the work happens.
        </p>
      )}

      <ul className="space-y-2">
        {operations.map((operation) => (
          <li
            key={operation.manufacturingOperationId}
            className="rounded-lg border p-4"
            data-testid="manufacturing-row"
          >
            <div className="flex flex-wrap items-baseline justify-between gap-2">
              <div className="flex flex-wrap items-baseline gap-2">
                <span className="font-medium">{operation.siteName}</span>

                <span className="text-xs text-muted-foreground">
                  {operation.siteCountryName}
                </span>

                <Badge variant="secondary" data-testid="manufacturing-operation">
                  {operation.operationDisplay}
                </Badge>
              </div>

              {/* Current or closed is stated on every row rather than implied
                  by the absence of an end date. */}
              {operation.isCurrent ? (
                <Badge data-testid="manufacturing-current">Current</Badge>
              ) : (
                <Badge variant="outline" data-testid="manufacturing-closed">
                  Until {operation.ceasedOn}
                </Badge>
              )}
            </div>

            <p className="mt-1 text-sm text-muted-foreground">
              Since {operation.effectiveFrom}
              {operation.siteIdentifiers.length > 0 && (
                <span data-testid="manufacturing-identifiers">
                  {" · "}
                  {operation.siteIdentifiers.join(" · ")}
                </span>
              )}
            </p>

            {operation.isCurrent && (
              <div className="mt-2">
                {closing === operation.manufacturingOperationId ? (
                  <CeaseRow
                    onSubmit={(ceasedOn) =>
                      cease.mutate(
                        {
                          operationId: operation.manufacturingOperationId,
                          ceasedOn,
                        },
                        { onSuccess: () => setClosing(undefined) },
                      )
                    }
                  />
                ) : (
                  <Button
                    size="sm"
                    variant="ghost"
                    onClick={() =>
                      setClosing(operation.manufacturingOperationId)
                    }
                    data-testid="cease-manufacturing"
                  >
                    No longer performed here
                  </Button>
                )}
              </div>
            )}
          </li>
        ))}
      </ul>

      {/* Said once, at the bottom, because it is the boundary the whole epic
          rests on: this section records what happens, not what is allowed. */}
      {current.length > 0 && (
        <p
          className="text-xs text-muted-foreground"
          data-testid="manufacturing-not-approval"
        >
          Recording an operation does not approve it. Whether a licence names
          these sites is a separate statement.
        </p>
      )}

      {recording && (
        <ManufacturingOperationDialog
          medicinalProductId={medicinalProductId}
          open
          onOpenChange={(open) => !open && setRecording(false)}
        />
      )}
    </section>
  );
}

/**
 * The date is asked for rather than assumed, for the reason a pack
 * authorisation's is: a transfer completed last quarter is recorded this one.
 */
function CeaseRow({ onSubmit }: { onSubmit(ceasedOn: string): void }) {
  const [ceasedOn, setCeasedOn] = useState("");

  return (
    <div className="flex flex-wrap items-end gap-2 rounded-md border border-dashed p-3">
      <div className="flex flex-col gap-1">
        <label htmlFor="ceased-on-input" className="text-xs">
          Stopped on
        </label>

        <input
          id="ceased-on-input"
          type="date"
          className="h-8 rounded-md border bg-transparent px-2 text-sm"
          value={ceasedOn}
          onChange={(event) => setCeasedOn(event.target.value)}
        />
      </div>

      <Button
        size="sm"
        disabled={ceasedOn === ""}
        onClick={() => onSubmit(ceasedOn)}
        data-testid="confirm-cease"
      >
        Close the period
      </Button>
    </div>
  );
}
