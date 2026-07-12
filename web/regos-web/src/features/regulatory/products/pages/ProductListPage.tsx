import { useState } from "react";
import { LoadingProducts } from "../components/LoadingProducts";
import { EmptyProductState } from "../components/EmptyProductState";
import { ProductLoadError } from "../components/ProductLoadError";
import { PageHeader } from "@/shared/components/PageHeader";
import { Button } from "@/components/ui/button";

import {
  ProductCard,
  RegisterProductDialog,
  useProducts,
} from "@/features/regulatory/products";

export function ProductListPage() {
  const { data, isLoading, error } = useProducts();

  const [dialogOpen, setDialogOpen] = useState(false);

  return (
    <>
      <PageHeader
        title="Products"
        description="Manage registered products."
        actions={
          <Button onClick={() => setDialogOpen(true)}>New Product</Button>
        }
      />

      <RegisterProductDialog open={dialogOpen} onOpenChange={setDialogOpen} />

      {isLoading && <LoadingProducts />}

      {!isLoading && error && <ProductLoadError retry={() => {}} />}

      {!isLoading && !error && data?.length === 0 && <EmptyProductState />}

      {!isLoading && !error && data && data.length > 0 && (
        <div className="space-y-3">
          {data.map((product) => (
            <ProductCard key={product.id} product={product} />
          ))}
        </div>
      )}
    </>
  );
}
