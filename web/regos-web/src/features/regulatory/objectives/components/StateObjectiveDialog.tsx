import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { StateObjectiveForm } from "./StateObjectiveForm";

interface StateObjectiveDialogProps {
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function StateObjectiveDialog({
  open,
  onOpenChange,
}: StateObjectiveDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>State an objective</DialogTitle>
        </DialogHeader>

        <StateObjectiveForm onSuccess={() => onOpenChange(false)} />
      </DialogContent>
    </Dialog>
  );
}
