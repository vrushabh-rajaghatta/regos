import { useState } from "react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";

import { PackSupplyDialog } from "./PackSupplyDialog";
import type { Pack } from "../types/Pack";
import { NO_SPECIAL_PRECAUTIONS } from "../types/Supply";

interface PackSupplyProps {
  medicinalProductId: string;
  pack: Pack;
}

/**
 * How this pack may be handed over, and how long it keeps.
 *
 * **Two facts, one line each, on the pack** — and being on the pack is the
 * decision (ADR-061 §1's discriminator, used a third time). A 16-tablet pack
 * may be general sale where a 100-tablet pack of the same tablets is
 * pharmacy-only, and the same tablets in an alu-alu blister and an HDPE bottle
 * keep for different lengths of time, because the container closure system is
 * what the stability data was generated against.
 *
 * **Silence and "none needed" read differently**, deliberately: an empty
 * storage list says nobody has stated any, where *"No special storage
 * precautions"* says somebody checked. The model refuses to blur them and so
 * does this.
 */
export function PackSupply({ medicinalProductId, pack }: PackSupplyProps) {
  const [editing, setEditing] = useState(false);

  const noneNeeded = pack.storageConditions.some(
    (condition) => condition.code === NO_SPECIAL_PRECAUTIONS,
  );

  return (
    <div className="mt-3 border-t pt-3" data-testid="pack-supply">
      <div className="flex flex-wrap items-center gap-2">
        {pack.legalStatusOfSupplyDisplay ? (
          <Badge variant="outline" data-testid="pack-legal-status">
            {pack.legalStatusOfSupplyDisplay}
          </Badge>
        ) : (
          <span
            className="text-xs text-muted-foreground"
            data-testid="pack-legal-status-unstated"
          >
            Not classified
          </span>
        )}

        {/* Read back in the words it was approved in — three years stays three
            years rather than becoming thirty-six months. */}
        {pack.shelfLifeValue !== null && (
          <span className="text-sm" data-testid="pack-shelf-life">
            Keeps {pack.shelfLifeValue} {pack.shelfLifeUnitDisplay}
          </span>
        )}

        <Button
          variant="ghost"
          size="sm"
          className="ml-auto"
          onClick={() => setEditing(true)}
          data-testid="edit-pack-supply"
        >
          Supply &amp; storage
        </Button>
      </div>

      {pack.storageConditions.length > 0 && (
        <p
          className="mt-1 text-sm text-muted-foreground"
          data-testid={
            noneNeeded ? "pack-storage-none-needed" : "pack-storage-conditions"
          }
        >
          {pack.storageConditions.map((x) => x.display).join(". ")}.
        </p>
      )}

      {pack.shelfLifeText && (
        <p
          className="mt-1 text-sm text-muted-foreground"
          data-testid="pack-shelf-life-text"
        >
          {pack.shelfLifeText}
        </p>
      )}

      {editing && (
        <PackSupplyDialog
          medicinalProductId={medicinalProductId}
          pack={pack}
          open
          onOpenChange={(open) => !open && setEditing(false)}
        />
      )}
    </div>
  );
}
