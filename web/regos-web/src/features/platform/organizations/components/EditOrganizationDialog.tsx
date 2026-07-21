import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import type { OrganizationDetails } from "../types/OrganizationDetails";
import { EditOrganizationForm } from "./EditOrganizationForm";

interface EditOrganizationDialogProps {
  organization: OrganizationDetails;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function EditOrganizationDialog({
  organization,
  open,
  onOpenChange,
}: EditOrganizationDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Edit Organization</DialogTitle>
        </DialogHeader>

        <EditOrganizationForm
          organization={organization}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
