import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { AddGlobalLabelForm } from "./AddGlobalLabelForm";

interface AddGlobalLabelDialogProps {
  globalProductId: string;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function AddGlobalLabelDialog({
  globalProductId,
  open,
  onOpenChange,
}: AddGlobalLabelDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Add global label</DialogTitle>
        </DialogHeader>

        <AddGlobalLabelForm
          globalProductId={globalProductId}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
