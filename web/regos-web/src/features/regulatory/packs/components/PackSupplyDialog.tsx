import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { PackSupplyForm } from "./PackSupplyForm";
import type { Pack } from "../types/Pack";

interface PackSupplyDialogProps {
  medicinalProductId: string;
  pack: Pack;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function PackSupplyDialog({
  medicinalProductId,
  pack,
  open,
  onOpenChange,
}: PackSupplyDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      {/* Capped and scrollable, the shape six other dialogs already use. It
          fitted on one screen until S004 added a fourth group; the Save button
          then fell below the fold with no way to reach it, which the browser
          proof found before a person did. */}
      <DialogContent className="max-h-[85vh] overflow-y-auto sm:max-w-md">
        <DialogHeader>
          {/* Names the pack rather than repeating a field label — "Legal
              status" and "Storage conditions" are both below, and a title
              echoing one makes getByLabel ambiguous. */}
          <DialogTitle>How is {pack.description} supplied?</DialogTitle>
        </DialogHeader>

        <PackSupplyForm
          medicinalProductId={medicinalProductId}
          pack={pack}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
