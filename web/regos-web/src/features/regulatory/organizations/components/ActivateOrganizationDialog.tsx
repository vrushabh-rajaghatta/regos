import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { useActivateOrganization } from "../hooks/useActivateOrganization";

interface ActivateOrganizationDialogProps {
  organizationId: string;
  legalName: string;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function ActivateOrganizationDialog({
  organizationId,
  legalName,
  open,
  onOpenChange,
}: ActivateOrganizationDialogProps) {
  const mutation = useActivateOrganization(organizationId);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Activate Organization?</DialogTitle>
        </DialogHeader>

        <p className="text-sm text-muted-foreground">
          {legalName} will be able to accept new users and regulatory work
          again.
        </p>

        {mutation.isError && (
          <p className="text-sm text-destructive" role="alert">
            {mutation.error.message}
          </p>
        )}

        <div className="flex justify-end gap-2">
          <Button
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={mutation.isPending}
          >
            Cancel
          </Button>

          <Button
            onClick={() =>
              mutation.mutate(undefined, {
                onSuccess: () => onOpenChange(false),
              })
            }
            disabled={mutation.isPending}
          >
            {mutation.isPending ? "Activating..." : "Activate"}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
