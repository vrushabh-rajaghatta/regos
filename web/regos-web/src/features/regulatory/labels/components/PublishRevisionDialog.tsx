import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { PublishRevisionForm } from "./PublishRevisionForm";

interface PublishRevisionDialogProps {
  localLabelId: string;
  revisionId: string;
  revisionNumber: number;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function PublishRevisionDialog({
  localLabelId,
  revisionId,
  revisionNumber,
  open,
  onOpenChange,
}: PublishRevisionDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Put revision {revisionNumber} in force</DialogTitle>
        </DialogHeader>

        <PublishRevisionForm
          localLabelId={localLabelId}
          revisionId={revisionId}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
