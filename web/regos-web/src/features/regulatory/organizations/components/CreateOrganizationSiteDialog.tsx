import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { CreateOrganizationSiteForm } from "./CreateOrganizationSiteForm";

interface CreateOrganizationSiteDialogProps {
  organizationId: string;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function CreateOrganizationSiteDialog({
  organizationId,
  open,
  onOpenChange,
}: CreateOrganizationSiteDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Add Site</DialogTitle>
        </DialogHeader>

        {open && (
          <CreateOrganizationSiteForm
            organizationId={organizationId}
            onSuccess={() => onOpenChange(false)}
          />
        )}
      </DialogContent>
    </Dialog>
  );
}
