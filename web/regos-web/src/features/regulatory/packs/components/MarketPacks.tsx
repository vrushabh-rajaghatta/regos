import { useState } from "react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";

import { PackContents } from "./PackContents";
import { PackDialog } from "./PackDialog";
import { PackStatusDialog } from "./PackStatusDialog";
import { packStatusLabel } from "../constants/packStatuses";
import { usePacks } from "../hooks/usePacks";
import type { Pack } from "../types/Pack";

interface MarketPacksProps {
  medicinalProductId: string;
}

/**
 * What this market actually sells.
 *
 * **A pack is how a medicine is supplied, not what it is** (ADR-061). The
 * presentation above says what the product *is* — 500 mg film-coated tablet —
 * and this says what you buy: a carton of 30 of them. Several packs is
 * ordinary, and the 30 and the 100 can be on sale and discontinued
 * independently, which is why each carries its own dated status.
 *
 * What is inside a pack, how it may be supplied and how long it lasts arrive in
 * S002 and S003.
 */
export function MarketPacks({ medicinalProductId }: MarketPacksProps) {
  const [adding, setAdding] = useState(false);
  const [editing, setEditing] = useState<Pack | undefined>();
  const [changingStatus, setChangingStatus] = useState<Pack | undefined>();

  const { data, isLoading, error } = usePacks(medicinalProductId);
  const packs = data ?? [];

  return (
    <section className="space-y-3" data-testid="market-packs">
      <div className="flex items-center justify-between">
        <h2 className="text-lg font-semibold">Packs</h2>

        <Button
          size="sm"
          variant="outline"
          onClick={() => setAdding(true)}
          data-testid="add-pack"
        >
          Add pack
        </Button>
      </div>

      {isLoading && (
        <p className="text-sm text-muted-foreground">Loading packs...</p>
      )}
      {error && <p className="text-sm text-destructive">Failed to load packs.</p>}

      {!isLoading && !error && packs.length === 0 && (
        <p
          className="rounded-lg border border-dashed p-6 text-center text-sm text-muted-foreground"
          data-testid="packs-empty"
        >
          Nothing is packed for sale here yet.
        </p>
      )}

      <ul className="space-y-3">
        {packs.map((pack) => (
          <li key={pack.id} className="rounded-lg border p-4" data-testid="pack-row">
            <div className="flex flex-wrap items-baseline gap-3">
              <span className="font-medium" data-testid="pack-description">
                {pack.description}
              </span>

              <Badge
                variant={
                  pack.currentMarketingStatus === "Marketed"
                    ? "default"
                    : "outline"
                }
                data-testid="pack-status"
              >
                {packStatusLabel(pack.currentMarketingStatus)}
              </Badge>

              <span className="text-xs text-muted-foreground">
                since {pack.currentMarketingStatusOccurredOn}
              </span>
            </div>

            <p className="mt-1 text-sm text-muted-foreground">
              {/* Null together or set together — the aggregate refuses half a
                  pack size, so "not stated" is a state rather than a gap. */}
              {pack.packSizeQuantity === null ? (
                <span data-testid="pack-size-unstated">Size not stated</span>
              ) : (
                <span data-testid="pack-size">
                  {pack.packSizeQuantity} {pack.packSizeUnitDisplay}
                </span>
              )}

              {pack.packCode && (
                <>
                  {" · "}
                  <span data-testid="pack-code">{pack.packCode}</span>
                </>
              )}
            </p>

            <div className="mt-2 flex flex-wrap gap-2">
              <Button
                variant="ghost"
                size="sm"
                onClick={() => setEditing(pack)}
                data-testid="correct-pack"
              >
                Correct
              </Button>

              <Button
                variant="ghost"
                size="sm"
                onClick={() => setChangingStatus(pack)}
                data-testid="change-pack-status"
              >
                Change status
              </Button>
            </div>

            {/* What is in the box, beneath what the box is. */}
            <PackContents packagedProductId={pack.id} />

            {/* Every dated point, never rewritten — a pack discontinued in 2024
                and entered today still says 2024. */}
            {pack.history.length > 1 && (
              <ul className="mt-2 border-t pt-2" data-testid="pack-history">
                {pack.history.map((entry) => (
                  <li
                    key={entry.id}
                    className="text-xs text-muted-foreground"
                    data-testid="pack-history-row"
                  >
                    {packStatusLabel(entry.status)} — {entry.occurredOn}
                    {entry.note && ` · ${entry.note}`}
                  </li>
                ))}
              </ul>
            )}
          </li>
        ))}
      </ul>

      <PackDialog
        medicinalProductId={medicinalProductId}
        open={adding}
        onOpenChange={setAdding}
      />

      {editing && (
        <PackDialog
          medicinalProductId={medicinalProductId}
          pack={editing}
          open
          onOpenChange={(open) => !open && setEditing(undefined)}
        />
      )}

      {changingStatus && (
        <PackStatusDialog
          medicinalProductId={medicinalProductId}
          packagedProductId={changingStatus.id}
          packDescription={changingStatus.description}
          open
          onOpenChange={(open) => !open && setChangingStatus(undefined)}
        />
      )}
    </section>
  );
}
