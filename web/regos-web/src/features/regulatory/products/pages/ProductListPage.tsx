import { useState } from "react";
import { useProducts } from "../hooks/useProducts";
import { ProductToolbar } from "../components/ProductToolbar";
import { RegisterProductDialog } from "../components/RegisterProductDialog";
import { LoadingProducts } from "../components/LoadingProducts";
import { EmptyProductState } from "../components/EmptyProductState";
import { ProductLoadError } from "../components/ProductLoadError";
import { ProductCard } from "../components/ProductCard";

export function ProductListPage() {
  const { data, isLoading, error } = useProducts();

  const [dialogOpen, setDialogOpen] = useState(false);

  return (
    <>
      <ProductToolbar onCreate={() => setDialogOpen(true)} />

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
