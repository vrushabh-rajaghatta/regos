import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import type { UserDetails } from "../types/UserDetails";
import { EditUserProfileForm } from "./EditUserProfileForm";

interface EditUserProfileDialogProps {
  user: UserDetails;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function EditUserProfileDialog({
  user,
  open,
  onOpenChange,
}: EditUserProfileDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Edit Profile</DialogTitle>
        </DialogHeader>

        {/* Remount per user so the form always starts from current values. */}
        <EditUserProfileForm
          key={`${user.id}:${user.email}:${user.firstName}:${user.lastName}`}
          user={user}
          onSuccess={() => onOpenChange(false)}
          onCancel={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
