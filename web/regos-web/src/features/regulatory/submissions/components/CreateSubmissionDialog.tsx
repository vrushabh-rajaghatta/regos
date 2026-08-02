import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { CreateSubmissionForm } from "./CreateSubmissionForm";

interface CreateSubmissionDialogProps {
  globalProductId: string;
  applicationId: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function CreateSubmissionDialog({
  globalProductId,
  applicationId,
  open,
  onOpenChange,
}: CreateSubmissionDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>New Submission</DialogTitle>
        </DialogHeader>

        <CreateSubmissionForm
          globalProductId={globalProductId}
          applicationId={applicationId}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
