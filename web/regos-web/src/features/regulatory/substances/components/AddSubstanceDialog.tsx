import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { AddSubstanceForm } from "./AddSubstanceForm";

interface AddSubstanceDialogProps {
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function AddSubstanceDialog({
  open,
  onOpenChange,
}: AddSubstanceDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-h-[85vh] overflow-y-auto sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Add substance</DialogTitle>
        </DialogHeader>

        <AddSubstanceForm onSuccess={() => onOpenChange(false)} />
      </DialogContent>
    </Dialog>
  );
}
