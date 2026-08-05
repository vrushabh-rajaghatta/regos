import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { ManufacturingOperationForm } from "./ManufacturingOperationForm";

interface ManufacturingOperationDialogProps {
  medicinalProductId: string;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function ManufacturingOperationDialog({
  medicinalProductId,
  open,
  onOpenChange,
}: ManufacturingOperationDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      {/* Capped and scrollable from the start, the shape seven other dialogs
          use — the pack supply dialog reached this the expensive way, by
          growing past the fold with its Save button below it. */}
      <DialogContent className="max-h-[85vh] overflow-y-auto sm:max-w-md">
        <DialogHeader>
          {/* Names the act, not a field below it: "Site" and "Operation" are
              both labels in the form, and a title echoing one makes getByLabel
              ambiguous. */}
          <DialogTitle>Record where work happens</DialogTitle>
        </DialogHeader>

        <ManufacturingOperationForm
          medicinalProductId={medicinalProductId}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
