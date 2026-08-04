import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { PresentationForm } from "./PresentationForm";
import type { Presentation } from "../types/Presentation";

interface PresentationDialogProps {
  medicinalProductId: string;
  /** Present when correcting one, absent when adding. */
  presentation?: Presentation;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function PresentationDialog({
  medicinalProductId,
  presentation,
  open,
  onOpenChange,
}: PresentationDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-h-[85vh] overflow-y-auto sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>
            {presentation ? "Edit presentation" : "Add presentation"}
          </DialogTitle>
        </DialogHeader>

        {/* Keyed by the presentation being edited: the form fills its defaults
            once, so reopening it on a different row must build a new one
            rather than reuse the previous row's values. */}
        <PresentationForm
          key={presentation?.presentationId ?? "new"}
          medicinalProductId={medicinalProductId}
          presentation={presentation}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
