import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { useDeactivateOrganization } from "../hooks/useDeactivateOrganization";

interface DeactivateOrganizationDialogProps {
  organizationId: string;
  legalName: string;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function DeactivateOrganizationDialog({
  organizationId,
  legalName,
  open,
  onOpenChange,
}: DeactivateOrganizationDialogProps) {
  const mutation = useDeactivateOrganization(organizationId);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Deactivate Organization?</DialogTitle>
        </DialogHeader>

        {/* Nothing is deleted, so the wording must not imply data loss. */}
        <p className="text-sm text-muted-foreground">
          {legalName} will no longer accept new users or regulatory work. Its
          existing records are kept.
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
            {mutation.isPending ? "Deactivating..." : "Deactivate"}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
