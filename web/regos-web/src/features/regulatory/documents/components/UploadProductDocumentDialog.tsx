import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { UploadProductDocumentForm } from "./UploadProductDocumentForm";

interface UploadProductDocumentDialogProps {
  globalProductId: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function UploadProductDocumentDialog({
  globalProductId,
  open,
  onOpenChange,
}: UploadProductDocumentDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Upload Document</DialogTitle>
        </DialogHeader>

        <UploadProductDocumentForm
          globalProductId={globalProductId}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
