import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { PackForm } from "./PackForm";
import type { Pack } from "../types/Pack";

interface PackDialogProps {
  medicinalProductId: string;
  pack?: Pack;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function PackDialog({
  medicinalProductId,
  pack,
  open,
  onOpenChange,
}: PackDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          {/* Deliberately not the word "Pack" on its own — the field below is
              labelled that, and a title repeating it makes getByLabel
              ambiguous, which is a wording defect before it is a test one. */}
          <DialogTitle>
            {pack ? "Correct this pack" : "Add a pack"}
          </DialogTitle>
        </DialogHeader>

        <PackForm
          medicinalProductId={medicinalProductId}
          pack={pack}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
