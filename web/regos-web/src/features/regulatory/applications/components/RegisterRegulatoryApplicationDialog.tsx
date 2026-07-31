import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { RegisterRegulatoryApplicationForm } from "./RegisterRegulatoryApplicationForm";

interface RegisterRegulatoryApplicationDialogProps {
  globalProductId: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function RegisterRegulatoryApplicationDialog({
  globalProductId,
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
          globalProductId={globalProductId}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
