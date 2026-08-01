import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { BeginMeetingForm } from "./BeginMeetingForm";

interface BeginMeetingDialogProps {
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function BeginMeetingDialog({
  open,
  onOpenChange,
}: BeginMeetingDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          {/* Distinct from the page's "Record meeting" button. */}
          <DialogTitle>A meeting with an authority</DialogTitle>
        </DialogHeader>

        <BeginMeetingForm onSuccess={() => onOpenChange(false)} />
      </DialogContent>
    </Dialog>
  );
}
