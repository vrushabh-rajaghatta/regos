import { useState } from "react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";

import { ComponentDialog } from "./ComponentDialog";
import { useComponents } from "../hooks/useComponents";
import { useMoveComponent } from "../hooks/useMoveComponent";
import { useRemoveComponent } from "../hooks/useRemoveComponent";
import type { Component } from "../types/Component";

interface MarketComponentsProps {
  medicinalProductId: string;
}

interface AddTarget {
  parentComponentId: string | null;
  parentName?: string;
}

/**
 * What the patient physically receives.
 *
 * **A flat list with indentation, not nested markup.** The server sends the
 * rows already in reading order with a depth on each, so the tree is rendered
 * by an indent rather than by recursion — one walk exists, on the server, and a
 * second one here would be a second answer to the same question.
 *
 * The move control is deliberately plain: **"Move out"** is the whole of it.
 * Every other move a user needs is expressible as remove-and-re-add at this
 * depth, and a drag-and-drop tree would be a lot of surface for a structure
 * that is three levels at most.
 */
export function MarketComponents({ medicinalProductId }: MarketComponentsProps) {
  const [adding, setAdding] = useState<AddTarget | undefined>();
  const [editing, setEditing] = useState<Component | undefined>();

  const { data, isLoading, error } = useComponents(medicinalProductId);
  const remove = useRemoveComponent(medicinalProductId);
  const move = useMoveComponent(medicinalProductId);

  const rows = data ?? [];

  return (
    <section className="space-y-3" data-testid="market-components">
      <div className="flex items-center justify-between">
        <h2 className="text-lg font-semibold">What the patient receives</h2>

        <Button
          size="sm"
          variant="outline"
          onClick={() => setAdding({ parentComponentId: null })}
          data-testid="add-component"
        >
          Add component
        </Button>
      </div>

      {isLoading && (
        <p className="text-sm text-muted-foreground">Loading components...</p>
      )}
      {error && (
        <p className="text-sm text-destructive">Failed to load components.</p>
      )}

      {!isLoading && !error && rows.length === 0 && (
        <p
          className="rounded-lg border border-dashed p-6 text-center text-sm text-muted-foreground"
          data-testid="components-empty"
        >
          Nothing recorded yet. Most products are a single article; a kit is
          where this earns its keep.
        </p>
      )}

      <ul className="space-y-1">
        {rows.map((component) => (
          <li
            key={component.componentId}
            className="flex items-center justify-between gap-3 rounded-md border p-3 text-sm"
            // Indented by the depth the server computed. Inline because the
            // value is data, not one of a fixed set of classes Tailwind could
            // have generated ahead of time.
            style={{ marginLeft: `${(component.depth - 1) * 1.5}rem` }}
            data-testid="component-row"
            data-depth={component.depth}
          >
            <div className="flex flex-wrap items-baseline gap-2">
              <span className="font-medium">{component.name}</span>

              <Badge variant="secondary">
                {component.componentType.display}
              </Badge>

              <span className="text-muted-foreground">
                ×{component.quantity}
              </span>

              {component.doseForm && (
                <span className="text-xs text-muted-foreground">
                  {component.doseForm.display}
                </span>
              )}
            </div>

            <div className="flex shrink-0 gap-1">
              <Button
                size="sm"
                variant="ghost"
                onClick={() =>
                  setAdding({
                    parentComponentId: component.componentId,
                    parentName: component.name,
                  })
                }
                data-testid="add-inside"
              >
                Add inside
              </Button>

              {component.parentComponentId !== null && (
                <Button
                  size="sm"
                  variant="ghost"
                  disabled={move.isPending}
                  onClick={() =>
                    move.mutate({
                      componentId: component.componentId,
                      newParentComponentId: null,
                    })
                  }
                  data-testid="move-out"
                >
                  Move out
                </Button>
              )}

              <Button
                size="sm"
                variant="ghost"
                onClick={() => setEditing(component)}
                data-testid="edit-component"
              >
                Edit
              </Button>

              <Button
                size="sm"
                variant="ghost"
                disabled={remove.isPending}
                onClick={() => remove.mutate(component.componentId)}
                data-testid="remove-component"
              >
                Remove
              </Button>
            </div>
          </li>
        ))}
      </ul>

      {/* Both refusals belong to the server, which is the only party that sees
          the whole tree — a component that still holds others, and a move that
          would create a cycle or go too deep. */}
      {remove.isError && (
        <p className="text-sm text-destructive" data-testid="component-remove-error">
          {(remove.error as Error).message}
        </p>
      )}

      {move.isError && (
        <p className="text-sm text-destructive" data-testid="component-move-error">
          {(move.error as Error).message}
        </p>
      )}

      <ComponentDialog
        medicinalProductId={medicinalProductId}
        parentComponentId={adding?.parentComponentId ?? null}
        parentName={adding?.parentName}
        open={adding !== undefined}
        onOpenChange={(open) => !open && setAdding(undefined)}
      />

      <ComponentDialog
        medicinalProductId={medicinalProductId}
        component={editing}
        open={editing !== undefined}
        onOpenChange={(open) => !open && setEditing(undefined)}
      />
    </section>
  );
}
