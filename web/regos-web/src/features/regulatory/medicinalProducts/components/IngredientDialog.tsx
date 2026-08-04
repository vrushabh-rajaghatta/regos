import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { IngredientForm } from "./IngredientForm";
import type { Ingredient } from "../types/Presentation";

interface IngredientDialogProps {
  medicinalProductId: string;
  presentationId: string;
  /** Present when correcting one, absent when adding. */
  ingredient?: Ingredient;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function IngredientDialog({
  medicinalProductId,
  presentationId,
  ingredient,
  open,
  onOpenChange,
}: IngredientDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-h-[85vh] overflow-y-auto sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>
            {ingredient ? "Edit ingredient" : "Add ingredient"}
          </DialogTitle>
        </DialogHeader>

        {/* Keyed by the ingredient: the form fills its defaults once, so
            reopening on a different row must build a new one rather than reuse
            the previous row's values. */}
        <IngredientForm
          key={ingredient?.ingredientId ?? "new"}
          medicinalProductId={medicinalProductId}
          presentationId={presentationId}
          ingredient={ingredient}
          onSuccess={() => onOpenChange(false)}
        />
      </DialogContent>
    </Dialog>
  );
}
