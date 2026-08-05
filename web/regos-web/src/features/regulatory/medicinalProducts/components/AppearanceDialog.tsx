import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { AppearanceForm } from "./AppearanceForm";
import type { Presentation } from "../types/Presentation";

interface AppearanceDialogProps {
  medicinalProductId: string;
  presentation: Presentation;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function AppearanceDialog({
  medicinalProductId,
  presentation,
  open,
  onOpenChange,
}: AppearanceDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          {/* Names the presentation rather than echoing a field label —
              "Colours", "Shape" and "Marking" are all below. */}
          <DialogTitle>What does {presentation.name} look like?</DialogTitle>
        </DialogHeader>

        <AppearanceForm
          medicinalProductId={medicinalProductId}
          presentation={presentation}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
