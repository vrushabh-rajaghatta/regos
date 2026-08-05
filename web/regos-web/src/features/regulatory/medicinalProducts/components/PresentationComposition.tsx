import { useState } from "react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";

import { IngredientDialog } from "./IngredientDialog";
import { useRemoveIngredient } from "../hooks/useRemoveIngredient";
import type { Ingredient, Presentation } from "../types/Presentation";

interface PresentationCompositionProps {
  medicinalProductId: string;
  presentation: Presentation;
}

/**
 * Renders a strength the way a person reads one.
 *
 * A point strength is *500 mg*; a concentration is *10 mg / 1 mL*. The two are
 * formatted here rather than by the server, because the server's job is to say
 * what the numbers are and this is a presentation decision.
 */
function formatStrength(ingredient: Ingredient): string {
  const strength = ingredient.strength;

  if (!strength) return "Not declared";

  const numerator = `${strength.numeratorValue} ${strength.numeratorUnit.display}`;

  return strength.denominatorValue === null || !strength.denominatorUnit
    ? numerator
    : `${numerator} / ${strength.denominatorValue} ${strength.denominatorUnit.display}`;
}

/**
 * What a presentation is made of.
 *
 * Actives first, then excipients — the order the server sorts in, and the order
 * a reader cares about. **The unfinished notice is a completeness statement,
 * not a refusal**: a composition without an active is accepted and said to be
 * incomplete, because requiring one on every edit would dictate the order a
 * user types a formulation in.
 */
export function PresentationComposition({
  medicinalProductId,
  presentation,
}: PresentationCompositionProps) {
  const [adding, setAdding] = useState(false);
  const [editing, setEditing] = useState<Ingredient | undefined>();

  const remove = useRemoveIngredient(
    medicinalProductId,
    presentation.presentationId,
  );

  return (
    <div className="mt-3 border-t pt-3" data-testid="presentation-composition">
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-medium text-muted-foreground">
          Composition
        </h3>

        <Button
          size="sm"
          variant="ghost"
          onClick={() => setAdding(true)}
          data-testid="add-ingredient"
        >
          Add ingredient
        </Button>
      </div>

      {presentation.ingredients.length === 0 && (
        <p
          className="mt-2 text-sm text-muted-foreground"
          data-testid="composition-empty"
        >
          Nothing recorded yet.
        </p>
      )}

      {presentation.ingredients.length > 0 &&
        !presentation.hasAnActiveIngredient && (
          <p
            className="mt-2 text-sm text-amber-600 dark:text-amber-500"
            data-testid="composition-incomplete"
          >
            No active ingredient yet — this composition does not say what the
            product works by.
          </p>
        )}

      <ul className="mt-2 space-y-1">
        {presentation.ingredients.map((ingredient) => (
          <li
            key={ingredient.ingredientId}
            className="flex items-center justify-between gap-3 rounded-md px-2 py-1 text-sm hover:bg-muted/50"
            data-testid="ingredient-row"
          >
            <div className="flex flex-wrap items-baseline gap-2">
              <span className="font-medium">{ingredient.substanceName}</span>

              <Badge
                variant={
                  ingredient.role === "Active" ? "default" : "secondary"
                }
              >
                {ingredient.role}
              </Badge>

              <span className="text-muted-foreground">
                {formatStrength(ingredient)}
              </span>

              {/* Shown only when it differs, so the row does not repeat
                  itself for the five seeded compounds whose name and INN
                  agree. */}
              {ingredient.substanceInn &&
                ingredient.substanceInn !== ingredient.substanceName && (
                  <span className="text-xs text-muted-foreground">
                    INN {ingredient.substanceInn}
                  </span>
                )}

              {/* Shown only when stated, and deliberately silent otherwise.
                  RegOS holds no provenance for anything recorded before
                  EPIC-010c, and a row reading "source: not stated" on every
                  ingredient would turn an honest absence into a nag.

                  This is where the substance comes from — not where the
                  finished product is made, which the market page answers
                  separately (ADR-063 §2). */}
              {ingredient.manufacturingSourceSiteName && (
                <span
                  className="text-xs text-muted-foreground"
                  data-testid="ingredient-source"
                >
                  from {ingredient.manufacturingSourceSiteName}
                </span>
              )}
            </div>

            <div className="flex shrink-0 gap-1">
              <Button
                size="sm"
                variant="ghost"
                onClick={() => setEditing(ingredient)}
                data-testid="edit-ingredient"
              >
                Edit
              </Button>

              <Button
                size="sm"
                variant="ghost"
                disabled={remove.isPending}
                onClick={() => remove.mutate(ingredient.ingredientId)}
                data-testid="remove-ingredient"
              >
                Remove
              </Button>
            </div>
          </li>
        ))}
      </ul>

      {/* The server refuses to leave a formulation with excipients and no
          active — it is the only party that knows the whole composition — so
          its reason is shown here rather than pre-empted on the button. */}
      {remove.isError && (
        <p
          className="mt-2 text-sm text-destructive"
          data-testid="remove-ingredient-error"
        >
          {(remove.error as Error).message}
        </p>
      )}

      <IngredientDialog
        medicinalProductId={medicinalProductId}
        presentationId={presentation.presentationId}
        open={adding}
        onOpenChange={setAdding}
      />

      <IngredientDialog
        medicinalProductId={medicinalProductId}
        presentationId={presentation.presentationId}
        ingredient={editing}
        open={editing !== undefined}
        onOpenChange={(open) => !open && setEditing(undefined)}
      />
    </div>
  );
}
