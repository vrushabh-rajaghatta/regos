import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { InviteUserForm } from "./InviteUserForm";

interface InviteUserDialogProps {
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function InviteUserDialog({ open, onOpenChange }: InviteUserDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Invite User</DialogTitle>
        </DialogHeader>

        <InviteUserForm onSuccess={() => onOpenChange(false)} />
      </DialogContent>
    </Dialog>
  );
}
