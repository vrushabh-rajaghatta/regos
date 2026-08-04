import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { PackStatusForm } from "./PackStatusForm";

interface PackStatusDialogProps {
  medicinalProductId: string;
  packagedProductId: string;
  packDescription: string;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function PackStatusDialog({
  medicinalProductId,
  packagedProductId,
  packDescription,
  open,
  onOpenChange,
}: PackStatusDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          {/* Names the pack, not the field below it. */}
          <DialogTitle>On sale — {packDescription}?</DialogTitle>
        </DialogHeader>

        <PackStatusForm
          medicinalProductId={medicinalProductId}
          packagedProductId={packagedProductId}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
