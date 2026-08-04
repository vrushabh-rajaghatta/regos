import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { PackageItemForm } from "./PackageItemForm";
import type { PackageItem } from "../types/PackageItem";

interface PackageItemDialogProps {
  packagedProductId: string;
  item?: PackageItem;
  parentPackageItemId: string | null;
  /** What the new layer goes inside, for the title. */
  parentDescription?: string;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function PackageItemDialog({
  packagedProductId,
  item,
  parentPackageItemId,
  parentDescription,
  open,
  onOpenChange,
}: PackageItemDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          {/* Deliberately never the bare word "Layer" — the field below is
              labelled that, and a title repeating it makes getByLabel
              ambiguous, which is a wording defect before it is a test one. */}
          <DialogTitle>
            {item
              ? "Correct this layer"
              : parentDescription
                ? `Put something inside the ${parentDescription}`
                : "What is in the box?"}
          </DialogTitle>
        </DialogHeader>

        <PackageItemForm
          packagedProductId={packagedProductId}
          item={item}
          parentPackageItemId={parentPackageItemId}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
