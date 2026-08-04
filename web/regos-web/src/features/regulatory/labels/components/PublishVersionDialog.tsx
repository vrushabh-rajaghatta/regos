import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { PublishVersionForm } from "./PublishVersionForm";

interface PublishVersionDialogProps {
  globalLabelId: string;
  versionId: string;
  versionNumber: number;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function PublishVersionDialog({
  globalLabelId,
  versionId,
  versionNumber,
  open,
  onOpenChange,
}: PublishVersionDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Publish version {versionNumber}</DialogTitle>
        </DialogHeader>

        <PublishVersionForm
          globalLabelId={globalLabelId}
          versionId={versionId}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
