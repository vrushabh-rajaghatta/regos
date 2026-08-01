import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { AddMarketForm } from "./AddMarketForm";

interface AddMarketDialogProps {
  globalProductId: string;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function AddMarketDialog({
  globalProductId,
  open,
  onOpenChange,
}: AddMarketDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Add market</DialogTitle>
        </DialogHeader>

        <AddMarketForm
          globalProductId={globalProductId}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
