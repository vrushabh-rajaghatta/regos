import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { RegisterRegulatoryApplicationForm } from "./RegisterRegulatoryApplicationForm";

interface RegisterRegulatoryApplicationDialogProps {
  productId: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function RegisterRegulatoryApplicationDialog({
  productId,
  open,
  onOpenChange,
}: RegisterRegulatoryApplicationDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>New Application</DialogTitle>
        </DialogHeader>

        <RegisterRegulatoryApplicationForm
          productId={productId}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
