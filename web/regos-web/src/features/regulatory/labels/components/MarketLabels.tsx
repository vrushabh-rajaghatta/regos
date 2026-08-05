import { useState } from "react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";

import { useDiscardLocalLabelDraft } from "../hooks/useDiscardLocalLabelDraft";
import { useLocalLabels } from "../hooks/useLocalLabels";
import { usePrintLocalLabelForPack } from "../hooks/usePrintLocalLabelForPack";
import { useStartLocalLabelRevision } from "../hooks/useStartLocalLabelRevision";
import { REGOS_INTERNAL } from "../types/GlobalLabel";

import { AddLocalLabelDialog } from "./AddLocalLabelDialog";
import { LocalLabelRevisions } from "./LocalLabelRevisions";
import { usePacks } from "@/features/regulatory/packs";

interface MarketLabelsProps {
  globalProductId: string;
  medicinalProductId: string;
}

/**
 * What this market's authority has approved.
 *
 * **Not the core label.** The company's scientific position lives on the
 * product; this is the regulatory artifact one authority approved, with its own
 * revision history, its own approval dates and its own pace (ADR-059).
 */
export function MarketLabels({
  globalProductId,
  medicinalProductId,
}: MarketLabelsProps) {
  const [adding, setAdding] = useState(false);

  const { data, isLoading, error } = useLocalLabels(medicinalProductId);
  const startRevision = useStartLocalLabelRevision();
  const discardDraft = useDiscardLocalLabelDraft();
  const printForPack = usePrintLocalLabelForPack();

  // The packs this market sells, so a carton can name the one it is printed
  // for (EPIC-010b D6 — the debt EPIC-018 named this epic as the milestone for).
  const { data: packs } = usePacks(medicinalProductId);

  const labels = data ?? [];

  return (
    <section className="space-y-3" data-testid="market-labels">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div>
          <h2 className="text-lg font-semibold">Local labels</h2>
          <p className="text-sm text-muted-foreground">
            What this market's authority approved — on its own clock, not the
            core label's.
          </p>
        </div>

        <Button
          size="sm"
          onClick={() => setAdding(true)}
          data-testid="add-local-label"
        >
          Add local label
        </Button>
      </div>

      {isLoading && (
        <p className="text-sm text-muted-foreground">Loading labels...</p>
      )}
      {error && <p className="text-sm text-destructive">Failed to load labels.</p>}

      {/* A refused start and a list that has not refreshed look identical
          otherwise — SC-106. */}
      {startRevision.isError && (
        <p
          className="text-sm text-destructive"
          data-testid="start-revision-error"
        >
          {(startRevision.error as Error).message}
        </p>
      )}

      {discardDraft.isError && (
        <p
          className="text-sm text-destructive"
          data-testid="discard-local-error"
        >
          {(discardDraft.error as Error).message}
        </p>
      )}

      {printForPack.isError && (
        <p
          className="text-sm text-destructive"
          data-testid="print-for-pack-error"
        >
          {(printForPack.error as Error).message}
        </p>
      )}

      {!isLoading && !error && labels.length === 0 && (
        <div
          className="rounded-lg border border-dashed p-6 text-center"
          data-testid="local-labels-empty"
        >
          <p className="text-sm text-muted-foreground">
            Nothing approved here yet. Add the document this market files.
          </p>
        </div>
      )}

      <ul className="space-y-3">
        {labels.map((label) => (
          <li
            key={label.id}
            className="rounded-lg border p-4"
            data-testid="local-label-row"
          >
            <div className="flex flex-wrap items-baseline gap-3">
              <span className="font-semibold">{label.labelTypeDisplay}</span>

              <Badge variant="secondary">
                {label.language.toUpperCase()}
              </Badge>

              {label.labelTypeSystem === REGOS_INTERNAL && (
                <span className="text-xs text-muted-foreground">
                  RegOS terminology
                </span>
              )}
            </div>

            <p className="mt-1 text-sm text-muted-foreground">
              {label.revisionInForceNumber === null ? (
                <span data-testid="local-nothing-in-force">
                  Nothing in force yet
                </span>
              ) : (
                <span data-testid="local-in-force">
                  Revision {label.revisionInForceNumber} in force — approved{" "}
                  {label.approvedOn}, effective {label.effectiveFrom}
                </span>
              )}
              {" · "}
              {label.revisionCount}{" "}
              {label.revisionCount === 1 ? "revision" : "revisions"}
            </p>

            {/*
              Offered on every label, not only on artwork. EPIC-018 D2 made
              artwork a label type rather than an aggregate and recorded the
              price — the moment a rule reads `if (Type == Artwork)`, that trade
              has stopped paying. A container label is printed per pack size
              anyway, so the branch would be wrong as well as expensive.

              The pack's own code and the artwork's DataCarrierCode stay
              separate: one is what the company registers, the other is what the
              approved artwork prints, and they are meant to be able to
              disagree.
            */}
            <div className="mt-2 flex flex-wrap items-center gap-2">
              <label
                htmlFor={`printed-for-${label.id}`}
                className="text-sm text-muted-foreground"
              >
                Printed for
              </label>

              <select
                id={`printed-for-${label.id}`}
                className="h-8 rounded-md border bg-transparent px-2 text-sm"
                value={label.packagedProductId ?? ""}
                onChange={(event) =>
                  printForPack.mutate({
                    localLabelId: label.id,
                    packagedProductId:
                      event.target.value === "" ? null : event.target.value,
                  })
                }
                data-testid="printed-for-pack"
              >
                <option value="">No particular pack</option>

                {(packs ?? []).map((pack) => (
                  <option key={pack.id} value={pack.id}>
                    {pack.description}
                  </option>
                ))}
              </select>
            </div>

            <div className="mt-2 flex flex-wrap items-center gap-2">
              {label.draftRevisionId ? (
                <>
                  <Badge variant="outline" data-testid="local-draft">
                    Revision {label.draftRevisionNumber} in preparation
                  </Badge>

                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => discardDraft.mutate(label.id)}
                    disabled={discardDraft.isPending}
                    data-testid="discard-local-draft"
                  >
                    Discard
                  </Button>
                </>
              ) : (
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => startRevision.mutate(label.id)}
                  disabled={startRevision.isPending}
                  data-testid="start-local-revision"
                >
                  Start next revision
                </Button>
              )}
            </div>

            <LocalLabelRevisions
              globalProductId={globalProductId}
              localLabelId={label.id}
            />
          </li>
        ))}
      </ul>

      <AddLocalLabelDialog
        medicinalProductId={medicinalProductId}
        open={adding}
        onOpenChange={setAdding}
      />
    </section>
  );
}
