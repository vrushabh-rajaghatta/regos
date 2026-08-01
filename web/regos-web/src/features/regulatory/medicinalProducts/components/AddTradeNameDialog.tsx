import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { AddTradeNameForm } from "./AddTradeNameForm";

interface AddTradeNameDialogProps {
  medicinalProductId: string;
  countryName: string;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function AddTradeNameDialog({
  medicinalProductId,
  countryName,
  open,
  onOpenChange,
}: AddTradeNameDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          {/* "Name in Canada", not "Trade name in Canada": the field below is
              already called Trade name, and a heading that repeats its field
              reads as a stutter — and cannot be told apart from it by anything
              addressing the page by accessible name. */}
          <DialogTitle>Name in {countryName}</DialogTitle>
        </DialogHeader>

        <AddTradeNameForm
          medicinalProductId={medicinalProductId}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
