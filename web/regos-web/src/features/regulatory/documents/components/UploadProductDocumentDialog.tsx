import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { UploadProductDocumentForm } from "./UploadProductDocumentForm";

interface UploadProductDocumentDialogProps {
  productId: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function UploadProductDocumentDialog({
  productId,
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
          productId={productId}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
