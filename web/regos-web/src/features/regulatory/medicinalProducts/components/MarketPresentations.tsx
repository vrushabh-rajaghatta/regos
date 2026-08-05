import { useState } from "react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";

import { AppearanceDialog } from "./AppearanceDialog";
import { PresentationComposition } from "./PresentationComposition";
import { PresentationDialog } from "./PresentationDialog";
import { usePresentations } from "../hooks/usePresentations";
import { REGOS_INTERNAL, type Presentation } from "../types/Presentation";

interface MarketPresentationsProps {
  medicinalProductId: string;
}

/**
 * What the product **is** in this market — as against what it is called, which
 * is the trade names above, and whether it is on sale, which is the history
 * below.
 *
 * Several presentations is ordinary: 10 mg, 20 mg and 40 mg tablets are one
 * commercial presence with three presentations, which is why this is a list and
 * not a panel of fields.
 */
export function MarketPresentations({
  medicinalProductId,
}: MarketPresentationsProps) {
  const [adding, setAdding] = useState(false);
  const [editing, setEditing] = useState<Presentation | undefined>();
  const [describing, setDescribing] = useState<Presentation | undefined>();

  const { data, isLoading, error } = usePresentations(medicinalProductId);
  const rows = data ?? [];

  return (
    <section className="space-y-3" data-testid="market-presentations">
      <div className="flex items-center justify-between">
        <h2 className="text-lg font-semibold">Presentations</h2>

        <Button
          size="sm"
          variant="outline"
          onClick={() => setAdding(true)}
          data-testid="add-presentation"
        >
          Add presentation
        </Button>
      </div>

      {isLoading && (
        <p className="text-sm text-muted-foreground">Loading presentations...</p>
      )}
      {error && (
        <p className="text-sm text-destructive">
          Failed to load presentations.
        </p>
      )}

      {!isLoading && !error && rows.length === 0 && (
        <p
          className="rounded-lg border border-dashed p-6 text-center text-sm text-muted-foreground"
          data-testid="presentations-empty"
        >
          Nothing recorded yet. A market presence exists long before the
          formulation is settled.
        </p>
      )}

      <ul className="space-y-2">
        {rows.map((presentation) => (
          <li
            key={presentation.presentationId}
            className="rounded-lg border p-4"
            data-testid="presentation-row"
          >
            <div className="flex items-start justify-between gap-3">
              <div>
                <div className="flex flex-wrap items-baseline gap-2">
                  <span className="font-medium">{presentation.name}</span>

                  <Badge variant="secondary">
                    {presentation.doseForm.display}
                  </Badge>

                  {presentation.unitOfPresentation && (
                    <span className="text-xs text-muted-foreground">
                      per {presentation.unitOfPresentation.display.toLowerCase()}
                    </span>
                  )}
                </div>

                <p className="mt-1 text-sm text-muted-foreground">
                  {presentation.routesOfAdministration.length > 0
                    ? presentation.routesOfAdministration
                        .map((route) => route.display)
                        .join(" · ")
                    : "No route recorded"}
                </p>

                {presentation.description && (
                  <p className="mt-1 text-sm">{presentation.description}</p>
                )}

                {/* Whose word this is, said out loud. Dose form and route are
                    EDQM Standard Terms in the real world; RegOS holds no such
                    licence and must not imply it does. */}
                {presentation.doseForm.system === REGOS_INTERNAL && (
                  <p className="mt-1 text-xs text-muted-foreground">
                    RegOS terminology
                  </p>
                )}
              </div>

              <div className="flex shrink-0 gap-1">
                <Button
                  size="sm"
                  variant="ghost"
                  onClick={() => setDescribing(presentation)}
                  data-testid="edit-appearance"
                >
                  Appearance
                </Button>

                <Button
                  size="sm"
                  variant="ghost"
                  onClick={() => setEditing(presentation)}
                  data-testid="edit-presentation"
                >
                  Edit
                </Button>
              </div>
            </div>

            {/* What it looks like, on the presentation and not on a pack: a
                tablet looks identical in a carton of 30 and a carton of 100
                (ADR-061 §1). */}
            {presentation.appearance.isStated && (
              <p className="mt-2 text-sm" data-testid="presentation-appearance">
                {[
                  presentation.appearance.colours
                    .map((colour) => colour.display)
                    .join(", "),
                  presentation.appearance.shape?.display,
                  presentation.appearance.imprint
                    ? `marked ${presentation.appearance.imprint}`
                    : null,
                ]
                  .filter(Boolean)
                  .join(" · ")}
              </p>
            )}

            {presentation.appearance.description && (
              <p
                className="mt-1 text-sm text-muted-foreground"
                data-testid="presentation-appearance-description"
              >
                {presentation.appearance.description}
              </p>
            )}

            {/* Inside the presentation, not beside it: a composition is what
                *this* administrable form is made of, and a market with three
                presentations has three of them. */}
            <PresentationComposition
              medicinalProductId={medicinalProductId}
              presentation={presentation}
            />
          </li>
        ))}
      </ul>

      {describing && (
        <AppearanceDialog
          medicinalProductId={medicinalProductId}
          presentation={describing}
          open
          onOpenChange={(open) => !open && setDescribing(undefined)}
        />
      )}

      <PresentationDialog
        medicinalProductId={medicinalProductId}
        open={adding}
        onOpenChange={setAdding}
      />

      <PresentationDialog
        medicinalProductId={medicinalProductId}
        presentation={editing}
        open={editing !== undefined}
        onOpenChange={(open) => !open && setEditing(undefined)}
      />
    </section>
  );
}
