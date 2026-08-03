import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { RecordApplicationNumberForm } from "./RecordApplicationNumberForm";

interface RecordApplicationNumberDialogProps {
  applicationId: string;
  currentNumber: string | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function RecordApplicationNumberDialog({
  applicationId,
  currentNumber,
  open,
  onOpenChange,
}: RecordApplicationNumberDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>
            {currentNumber ? "Application Number" : "Record Application Number"}
          </DialogTitle>
        </DialogHeader>

        <RecordApplicationNumberForm
          applicationId={applicationId}
          currentNumber={currentNumber}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
