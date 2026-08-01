import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { RaiseQuestionForm } from "./RaiseQuestionForm";

interface RaiseQuestionDialogProps {
  correspondenceId: string;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function RaiseQuestionDialog({
  correspondenceId,
  open,
  onOpenChange,
}: RaiseQuestionDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          {/* "A question from the authority", not "Raise question": the
              section's button is already called that. */}
          <DialogTitle>A question from the authority</DialogTitle>
        </DialogHeader>

        <RaiseQuestionForm
          correspondenceId={correspondenceId}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
