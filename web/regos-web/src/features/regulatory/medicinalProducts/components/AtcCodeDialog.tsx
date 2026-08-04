import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { AtcCodeForm } from "./AtcCodeForm";

interface AtcCodeDialogProps {
  medicinalProductId: string;
  currentAtcCode: string | null;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function AtcCodeDialog({
  medicinalProductId,
  currentAtcCode,
  open,
  onOpenChange,
}: AtcCodeDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Therapeutic classification</DialogTitle>
        </DialogHeader>

        <AtcCodeForm
          medicinalProductId={medicinalProductId}
          currentAtcCode={currentAtcCode}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
