import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { ChangeMarketStatusForm } from "./ChangeMarketStatusForm";

interface ChangeMarketStatusDialogProps {
  medicinalProductId: string;
  countryName: string;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function ChangeMarketStatusDialog({
  medicinalProductId,
  countryName,
  open,
  onOpenChange,
}: ChangeMarketStatusDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          {/* Names the market, not the field below it — the same collision the
              trade-name dialog had, avoided rather than repeated. */}
          <DialogTitle>On sale in {countryName}?</DialogTitle>
        </DialogHeader>

        <ChangeMarketStatusForm
          medicinalProductId={medicinalProductId}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
