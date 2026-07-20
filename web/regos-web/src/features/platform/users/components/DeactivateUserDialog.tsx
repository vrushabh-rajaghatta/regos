import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { useDeactivateUser } from "../hooks/useDeactivateUser";

interface DeactivateUserDialogProps {
  userId: string;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function DeactivateUserDialog({
  userId,
  open,
  onOpenChange,
}: DeactivateUserDialogProps) {
  const mutation = useDeactivateUser(userId);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Deactivate User?</DialogTitle>
        </DialogHeader>

        {/* Nothing is deleted, so the wording must not imply data loss. */}
        <p className="text-sm text-muted-foreground">
          This user will no longer be able to access RegOS. Their account and
          history are kept, and you can reactivate them at any time.
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
