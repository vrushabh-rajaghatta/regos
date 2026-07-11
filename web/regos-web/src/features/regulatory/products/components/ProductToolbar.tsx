import { Button } from "@/components/ui/button";

interface ProductToolbarProps {
  onCreate(): void;
}

export function ProductToolbar({ onCreate }: ProductToolbarProps) {
  return (
    <div className="mb-6 flex items-center justify-between">
      <h1 className="text-2xl font-semibold">Products</h1>

      <Button onClick={onCreate}>New Product</Button>
    </div>
  );
}
