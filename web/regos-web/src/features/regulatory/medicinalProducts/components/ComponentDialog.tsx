import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { ComponentForm } from "./ComponentForm";
import type { Component } from "../types/Component";

interface ComponentDialogProps {
  medicinalProductId: string;
  /** Present when correcting one, absent when adding. */
  component?: Component;
  /** Where a new one goes. Null puts it at the top level. */
  parentComponentId?: string | null;
  parentName?: string;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function ComponentDialog({
  medicinalProductId,
  component,
  parentComponentId = null,
  parentName,
  open,
  onOpenChange,
}: ComponentDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-h-[85vh] overflow-y-auto sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>
            {component ? "Edit component" : "Add component"}
          </DialogTitle>
        </DialogHeader>

        {/* Keyed by what it is editing *and* where a new one is going: the form
            fills its defaults once, so reopening it on a different row — or
            inside a different holder — must build a new one. */}
        <ComponentForm
          key={component?.componentId ?? `new-${parentComponentId ?? "top"}`}
          medicinalProductId={medicinalProductId}
          component={component}
          parentComponentId={parentComponentId}
          parentName={parentName}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
