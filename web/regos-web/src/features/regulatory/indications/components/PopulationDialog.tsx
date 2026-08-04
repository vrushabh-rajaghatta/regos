import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import type { Population } from "../types/Indication";

import { PopulationForm } from "./PopulationForm";

interface PopulationDialogProps {
  indicationId: string;
  population?: Population;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function PopulationDialog({
  indicationId,
  population,
  open,
  onOpenChange,
}: PopulationDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-h-[85vh] overflow-y-auto sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>
            {population ? "Correct population" : "Add population"}
          </DialogTitle>
        </DialogHeader>

        <PopulationForm
          indicationId={indicationId}
          population={population}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
