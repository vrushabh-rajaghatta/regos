import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { MarketActivationForm } from "./MarketActivationForm";

interface MarketActivationDialogProps {
  medicinalProductId: string;
  countryName: string;
  /** True when restoring a retired record, false when retiring an active one. */
  active: boolean;
  registrationCount: number;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function MarketActivationDialog({
  medicinalProductId,
  countryName,
  active,
  registrationCount,
  open,
  onOpenChange,
}: MarketActivationDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          {/* Names the market and the direction, never a field below it. */}
          <DialogTitle>
            {active ? `Restore ${countryName}` : `Retire ${countryName}`}
          </DialogTitle>
        </DialogHeader>

        <MarketActivationForm
          medicinalProductId={medicinalProductId}
          active={active}
          registrationCount={registrationCount}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
