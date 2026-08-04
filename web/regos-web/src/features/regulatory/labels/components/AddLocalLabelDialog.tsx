import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { AddLocalLabelForm } from "./AddLocalLabelForm";

interface AddLocalLabelDialogProps {
  medicinalProductId: string;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function AddLocalLabelDialog({
  medicinalProductId,
  open,
  onOpenChange,
}: AddLocalLabelDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Add local label</DialogTitle>
        </DialogHeader>

        <AddLocalLabelForm
          medicinalProductId={medicinalProductId}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
