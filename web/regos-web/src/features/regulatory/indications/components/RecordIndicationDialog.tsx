import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { RecordIndicationForm } from "./RecordIndicationForm";

interface RecordIndicationDialogProps {
  medicinalProductId: string;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function RecordIndicationDialog({
  medicinalProductId,
  open,
  onOpenChange,
}: RecordIndicationDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Record indication</DialogTitle>
        </DialogHeader>

        <RecordIndicationForm
          medicinalProductId={medicinalProductId}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
