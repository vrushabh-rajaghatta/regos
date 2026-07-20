import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { useArchiveProduct } from "../hooks/useArchiveProduct";
import type { ProductDetails } from "../types/ProductDetails";

interface ArchiveProductDialogProps {
  product: ProductDetails;
  open: boolean;
  onOpenChange(open: boolean): void;
}

export function ArchiveProductDialog({
  product,
  open,
  onOpenChange,
}: ArchiveProductDialogProps) {
  const mutation = useArchiveProduct(product.id);

  async function onConfirm() {
    await mutation.mutateAsync();

    onOpenChange(false);
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Archive {product.name}?</DialogTitle>
        </DialogHeader>

        {/* Worded to avoid implying data loss: archiving retires the product
            from new work, it does not delete it or affect existing records. */}
        <p className="text-sm text-muted-foreground">
          This product will no longer be available for new regulatory work.
          Existing applications, submissions and documents are not affected, and
          the product remains viewable.
        </p>

        {mutation.isError && (
          <p className="text-sm text-destructive" role="alert">
            {mutation.error.message}
          </p>
        )}

        <div className="flex justify-end gap-2">
          <Button
            type="button"
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={mutation.isPending}
          >
            Cancel
          </Button>

          <Button
            type="button"
            onClick={onConfirm}
            disabled={mutation.isPending}
            data-testid="confirm-archive"
          >
            {mutation.isPending ? "Archiving..." : "Archive Product"}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
