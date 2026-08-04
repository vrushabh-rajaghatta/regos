import { useState } from "react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";

import { PackageItemDialog } from "./PackageItemDialog";
import { useMovePackageItem } from "../hooks/useMovePackageItem";
import { usePackageItems } from "../hooks/usePackageItems";
import { useRemovePackageItem } from "../hooks/useRemovePackageItem";
import type { PackageItem } from "../types/PackageItem";

interface PackContentsProps {
  packagedProductId: string;
}

/**
 * What is inside one pack — the carton, the blisters inside it.
 *
 * **The indentation is the server's**, not the browser's: `depth` comes from
 * the same `PackagingTree` the domain rules use, so what a person sees and what
 * the guard measured cannot drift apart.
 *
 * A pack may be four layers deep, one more than a component tree. That is a
 * domain rule rather than a schema limit, and the refusal says so.
 */
export function PackContents({ packagedProductId }: PackContentsProps) {
  const [adding, setAdding] = useState<PackageItem | null | undefined>();
  const [editing, setEditing] = useState<PackageItem | undefined>();

  const { data, isLoading, error } = usePackageItems(packagedProductId);
  const move = useMovePackageItem(packagedProductId);
  const remove = useRemovePackageItem(packagedProductId);

  const items = data ?? [];

  return (
    <div className="mt-3 border-t pt-3" data-testid="pack-contents">
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-medium text-muted-foreground">
          What's inside
        </h3>

        <Button
          variant="ghost"
          size="sm"
          onClick={() => setAdding(null)}
          data-testid="add-package-item"
        >
          Add layer
        </Button>
      </div>

      {isLoading && (
        <p className="text-sm text-muted-foreground">Loading contents...</p>
      )}
      {error && (
        <p className="text-sm text-destructive">Failed to load contents.</p>
      )}

      {/* Both refusals surface here rather than inside a dialog, because both
          are provoked by controls on this list (SC-106). */}
      {move.isError && (
        <p className="text-sm text-destructive" data-testid="move-item-error">
          {(move.error as Error).message}
        </p>
      )}
      {remove.isError && (
        <p className="text-sm text-destructive" data-testid="remove-item-error">
          {(remove.error as Error).message}
        </p>
      )}

      {!isLoading && !error && items.length === 0 && (
        <p
          className="py-3 text-sm text-muted-foreground"
          data-testid="pack-contents-empty"
        >
          Nothing recorded inside this pack yet.
        </p>
      )}

      <ul className="mt-1 space-y-1">
        {items.map((item) => (
          <li
            key={item.id}
            className="flex flex-wrap items-baseline gap-2 py-1 text-sm"
            style={{ paddingLeft: `${(item.depth - 1) * 1.5}rem` }}
            data-testid="package-item-row"
            data-depth={item.depth}
          >
            <span data-testid="package-item-quantity">
              {item.quantity}
              {item.unitOfPresentationDisplay
                ? ` ${item.unitOfPresentationDisplay}`
                : ""}
            </span>

            <span className="font-medium" data-testid="package-item-type">
              {item.itemTypeDisplay}
            </span>

            {/* The attribute that makes this not a component. */}
            {item.materialDisplay && (
              <Badge variant="secondary" data-testid="package-item-material">
                {item.materialDisplay}
              </Badge>
            )}

            {item.description && (
              <span className="text-xs text-muted-foreground">
                {item.description}
              </span>
            )}

            <span className="ml-auto flex gap-1">
              <Button
                variant="ghost"
                size="sm"
                onClick={() => setAdding(item)}
                data-testid="add-inside"
              >
                Add inside
              </Button>

              <Button
                variant="ghost"
                size="sm"
                onClick={() => setEditing(item)}
                data-testid="correct-package-item"
              >
                Correct
              </Button>

              {item.parentPackageItemId && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() =>
                    move.mutate({
                      packageItemId: item.id,
                      newParentPackageItemId: null,
                    })
                  }
                  data-testid="lift-package-item"
                >
                  Lift out
                </Button>
              )}

              <Button
                variant="ghost"
                size="sm"
                onClick={() => remove.mutate(item.id)}
                data-testid="remove-package-item"
              >
                Remove
              </Button>
            </span>
          </li>
        ))}
      </ul>

      {adding !== undefined && (
        <PackageItemDialog
          packagedProductId={packagedProductId}
          parentPackageItemId={adding?.id ?? null}
          parentDescription={adding?.itemTypeDisplay.toLowerCase()}
          open
          onOpenChange={(open) => !open && setAdding(undefined)}
        />
      )}

      {editing && (
        <PackageItemDialog
          packagedProductId={packagedProductId}
          item={editing}
          parentPackageItemId={editing.parentPackageItemId}
          open
          onOpenChange={(open) => !open && setEditing(undefined)}
        />
      )}
    </div>
  );
}
