import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { CreateOrganizationDivisionForm } from "./CreateOrganizationDivisionForm";

interface CreateOrganizationDivisionDialogProps {
  organizationId: string;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function CreateOrganizationDivisionDialog({
  organizationId,
  open,
  onOpenChange,
}: CreateOrganizationDivisionDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Add Division</DialogTitle>
        </DialogHeader>

        {/* Mounted only while open, so each attempt starts clean rather than
            keeping the last one's values and errors. */}
        {open && (
          <CreateOrganizationDivisionForm
            organizationId={organizationId}
            onSuccess={() => onOpenChange(false)}
          />
        )}
      </DialogContent>
    </Dialog>
  );
}
