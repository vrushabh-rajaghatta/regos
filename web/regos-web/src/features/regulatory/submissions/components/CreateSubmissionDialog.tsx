import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { CreateSubmissionForm } from "./CreateSubmissionForm";

interface CreateSubmissionDialogProps {
  productId: string;
  applicationId: string;
  authorityId: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function CreateSubmissionDialog({
  productId,
  applicationId,
  authorityId,
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
          productId={productId}
          applicationId={applicationId}
          authorityId={authorityId}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
