import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { AddOrganizationIdentifierForm } from "./AddOrganizationIdentifierForm";

interface AddOrganizationIdentifierDialogProps {
  organizationId: string;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function AddOrganizationIdentifierDialog({
  organizationId,
  open,
  onOpenChange,
}: AddOrganizationIdentifierDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      {/* Mounted only while open, so the form starts empty each time rather
          than keeping the last attempt's values and errors. */}
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Record Identifier</DialogTitle>
        </DialogHeader>

        {open && (
          <AddOrganizationIdentifierForm
            organizationId={organizationId}
            onSuccess={() => onOpenChange(false)}
          />
        )}
      </DialogContent>
    </Dialog>
  );
}
