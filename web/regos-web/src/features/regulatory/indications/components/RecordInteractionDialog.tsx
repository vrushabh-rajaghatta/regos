import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { RecordInteractionForm } from "./RecordInteractionForm";

interface RecordInteractionDialogProps {
  medicinalProductId: string;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function RecordInteractionDialog({
  medicinalProductId,
  open,
  onOpenChange,
}: RecordInteractionDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-h-[85vh] overflow-y-auto sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Record interaction</DialogTitle>
        </DialogHeader>

        <RecordInteractionForm
          medicinalProductId={medicinalProductId}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
