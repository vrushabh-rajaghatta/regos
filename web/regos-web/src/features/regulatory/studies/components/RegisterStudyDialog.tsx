import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { RegisterStudyForm } from "./RegisterStudyForm";

interface RegisterStudyDialogProps {
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function RegisterStudyDialog({
  open,
  onOpenChange,
}: RegisterStudyDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Register study</DialogTitle>
        </DialogHeader>

        <RegisterStudyForm onSuccess={() => onOpenChange(false)} />
      </DialogContent>
    </Dialog>
  );
}
