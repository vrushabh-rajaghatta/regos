import { useState } from "react";
import { useParams } from "react-router-dom";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";

import { AddGlobalLabelDialog } from "../components/AddGlobalLabelDialog";
import { GlobalLabelVersions } from "../components/GlobalLabelVersions";
import { useDiscardGlobalLabelDraft } from "../hooks/useDiscardGlobalLabelDraft";
import { useGlobalLabels } from "../hooks/useGlobalLabels";
import { useStartGlobalLabelDraft } from "../hooks/useStartGlobalLabelDraft";
import { REGOS_INTERNAL } from "../types/GlobalLabel";

/**
 * The labels a company holds for one product, above any market.
 *
 * Screen word **global label**; the domain type is `GlobalLabel` (ADR-059 §8).
 * What each market says is a *local* label, and arrives in S002 — this page
 * deliberately says nothing about markets.
 */
export function ProductLabelsPage() {
  const { globalProductId = "" } = useParams();
  const [adding, setAdding] = useState(false);

  const { data, isLoading, error } = useGlobalLabels(globalProductId);
  const startDraft = useStartGlobalLabelDraft();
  const discardDraft = useDiscardGlobalLabelDraft();

  const labels = data ?? [];

  return (
    <Page>
      <PageHeader
        title="Global labels"
        description="The core data sheet and its siblings — held for the product, above any market."
        actions={
          <Button onClick={() => setAdding(true)} data-testid="add-global-label">
            Add label
          </Button>
        }
      />

      {isLoading && <p className="text-muted-foreground">Loading labels...</p>}
      {error && <p className="text-destructive">Failed to load labels.</p>}

      {/* A refused draft and a list that has not refreshed look identical
          otherwise — SC-106. */}
      {startDraft.isError && (
        <p className="text-sm text-destructive" data-testid="start-draft-error">
          {(startDraft.error as Error).message}
        </p>
      )}

      {discardDraft.isError && (
        <p className="text-sm text-destructive" data-testid="discard-error">
          {(discardDraft.error as Error).message}
        </p>
      )}

      {!isLoading && !error && labels.length === 0 && (
        <div
          className="rounded-lg border border-dashed p-8 text-center"
          data-testid="global-labels-empty"
        >
          <h3 className="text-lg font-semibold">No labels yet.</h3>
          <p className="mt-1 text-sm text-muted-foreground">
            Add the core data sheet this product is written from.
          </p>
        </div>
      )}

      <ul className="space-y-3">
        {labels.map((label) => (
          <li
            key={label.id}
            className="rounded-lg border p-4"
            data-testid="global-label-row"
          >
            <div className="flex flex-wrap items-baseline gap-3">
              <span className="font-semibold">{label.name}</span>

              <Badge variant="secondary">{label.labelTypeDisplay}</Badge>

              {/* Whose word this is, said out loud (ADR-058 §6). */}
              {label.labelTypeSystem === REGOS_INTERNAL && (
                <span className="text-xs text-muted-foreground">
                  RegOS terminology
                </span>
              )}
            </div>

            <p className="mt-1 text-sm text-muted-foreground">
              {label.versionInForceNumber === null ? (
                // Not an error state. A label exists from the moment someone
                // starts writing it, and says so rather than showing a blank.
                <span data-testid="label-nothing-in-force">
                  Nothing in force yet
                </span>
              ) : (
                <span data-testid="label-in-force">
                  Version {label.versionInForceNumber} in force since{" "}
                  {label.effectiveFrom}
                </span>
              )}
              {" · "}
              {label.versionCount}{" "}
              {label.versionCount === 1 ? "version" : "versions"}
            </p>

            <div className="mt-2 flex flex-wrap items-center gap-2">
              {label.draftVersionId ? (
                <>
                  <Badge variant="outline" data-testid="label-draft">
                    Version {label.draftVersionNumber} in draft
                  </Badge>

                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => discardDraft.mutate(label.id)}
                    disabled={discardDraft.isPending}
                    data-testid="discard-draft"
                  >
                    Discard draft
                  </Button>
                </>
              ) : (
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => startDraft.mutate(label.id)}
                  disabled={startDraft.isPending}
                  data-testid="start-draft"
                >
                  Start next version
                </Button>
              )}
            </div>

            <GlobalLabelVersions
              globalProductId={globalProductId}
              globalLabelId={label.id}
            />
          </li>
        ))}
      </ul>

      <AddGlobalLabelDialog
        globalProductId={globalProductId}
        open={adding}
        onOpenChange={setAdding}
      />
    </Page>
  );
}
