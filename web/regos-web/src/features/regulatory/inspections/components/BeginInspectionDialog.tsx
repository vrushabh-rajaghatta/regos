import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { BeginInspectionForm } from "./BeginInspectionForm";

interface BeginInspectionDialogProps {
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function BeginInspectionDialog({
  open,
  onOpenChange,
}: BeginInspectionDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>An inspection by an authority</DialogTitle>
        </DialogHeader>

        <BeginInspectionForm onSuccess={() => onOpenChange(false)} />
      </DialogContent>
    </Dialog>
  );
}
