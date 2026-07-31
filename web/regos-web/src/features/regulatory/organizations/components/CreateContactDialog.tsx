import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { CreateContactForm } from "./CreateContactForm";

interface CreateContactDialogProps {
  organizationId: string;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function CreateContactDialog({
  organizationId,
  open,
  onOpenChange,
}: CreateContactDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Add Contact</DialogTitle>
        </DialogHeader>

        {open && (
          <CreateContactForm
            organizationId={organizationId}
            onSuccess={() => onOpenChange(false)}
          />
        )}
      </DialogContent>
    </Dialog>
  );
}
