import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import type { StatementKind } from "../types/StatementKind";

import { RecordStatementForm } from "./RecordStatementForm";

interface RecordStatementDialogProps {
  kind: Exclude<StatementKind, "indications">;
  title: string;
  medicinalProductId: string;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function RecordStatementDialog({
  kind,
  title,
  medicinalProductId,
  open,
  onOpenChange,
}: RecordStatementDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
        </DialogHeader>

        <RecordStatementForm
          kind={kind}
          medicinalProductId={medicinalProductId}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
