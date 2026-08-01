import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { RecordCorrespondenceForm } from "./RecordCorrespondenceForm";

interface RecordCorrespondenceDialogProps {
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function RecordCorrespondenceDialog({
  open,
  onOpenChange,
}: RecordCorrespondenceDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          {/* "New correspondence", not "Log correspondence": the page's button
              is already called that, and a dialog that echoes its trigger
              gives two controls one accessible name. */}
          <DialogTitle>New correspondence</DialogTitle>
        </DialogHeader>

        <RecordCorrespondenceForm onSuccess={() => onOpenChange(false)} />
      </DialogContent>
    </Dialog>
  );
}
