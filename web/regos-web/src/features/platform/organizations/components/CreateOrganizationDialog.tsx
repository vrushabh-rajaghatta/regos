import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { CreateOrganizationForm } from "./CreateOrganizationForm";

interface CreateOrganizationDialogProps {
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function CreateOrganizationDialog({
  open,
  onOpenChange,
}: CreateOrganizationDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Create Organization</DialogTitle>
        </DialogHeader>

        <CreateOrganizationForm onSuccess={() => onOpenChange(false)} />
      </DialogContent>
    </Dialog>
  );
}
