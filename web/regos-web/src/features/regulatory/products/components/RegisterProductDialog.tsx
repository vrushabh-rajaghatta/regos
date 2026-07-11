import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { RegisterProductForm } from "./RegisterProductForm";

interface RegisterProductDialogProps {
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function RegisterProductDialog({
  open,
  onOpenChange,
}: RegisterProductDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Register Product</DialogTitle>
        </DialogHeader>

        <RegisterProductForm onSuccess={() => onOpenChange(false)} />
      </DialogContent>
    </Dialog>
  );
}
