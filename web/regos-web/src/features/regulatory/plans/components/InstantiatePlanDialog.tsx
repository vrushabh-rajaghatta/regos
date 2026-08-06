import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { InstantiatePlanForm } from "./InstantiatePlanForm";

interface InstantiatePlanDialogProps {
  objectiveId: string;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function InstantiatePlanDialog({
  objectiveId,
  open,
  onOpenChange,
}: InstantiatePlanDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Create a plan</DialogTitle>
        </DialogHeader>

        <InstantiatePlanForm
          objectiveId={objectiveId}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
