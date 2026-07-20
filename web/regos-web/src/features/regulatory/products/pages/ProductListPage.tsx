import { useState } from "react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { PageHeader } from "@/shared/components/PageHeader";

import {
  ProductCard,
  RegisterProductDialog,
  useProducts,
} from "@/features/regulatory/products";

import { EmptyProductState } from "../components/EmptyProductState";
import { LoadingProducts } from "../components/LoadingProducts";
import { ProductLoadError } from "../components/ProductLoadError";
import { PRODUCT_STATUSES } from "../constants/productStatuses";
import { PRODUCT_TYPES } from "../constants/productTypes";

const ALL = "all";

export function ProductListPage() {
  const [dialogOpen, setDialogOpen] = useState(false);
  const [search, setSearch] = useState("");
  const [type, setType] = useState<string>(ALL);
  const [status, setStatus] = useState<string>(ALL);
  const [page, setPage] = useState(1);

  const { data, isPending, isError, refetch } = useProducts({
    search: search || undefined,
    type: type === ALL ? undefined : type,
    status: status === ALL ? undefined : status,
    page,
  });

  // Any filter change invalidates the current page number.
  function change<T>(set: (value: T) => void) {
    return (value: T) => {
      set(value);
      setPage(1);
    };
  }

  const totalPages = data ? Math.max(1, Math.ceil(data.totalCount / data.pageSize)) : 1;

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

      <div className="mb-4 flex flex-wrap gap-2">
        <Input
          aria-label="Search products"
          placeholder="Search by code or name"
          className="max-w-xs"
          value={search}
          onChange={(event) => change(setSearch)(event.target.value)}
        />

        <Select value={type} onValueChange={(value) => change(setType)(value ?? ALL)}>
          <SelectTrigger aria-label="Filter by type" className="w-48">
            <SelectValue placeholder="All types" />
          </SelectTrigger>

          <SelectContent>
            <SelectItem value={ALL}>All types</SelectItem>
            {PRODUCT_TYPES.map((option) => (
              <SelectItem key={option.value} value={option.value}>
                {option.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select value={status} onValueChange={(value) => change(setStatus)(value ?? ALL)}>
          <SelectTrigger aria-label="Filter by status" className="w-48">
            <SelectValue placeholder="All statuses" />
          </SelectTrigger>

          <SelectContent>
            <SelectItem value={ALL}>All statuses</SelectItem>
            {PRODUCT_STATUSES.map((option) => (
              <SelectItem key={option.value} value={option.value}>
                {option.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {/* Loading / Error / Empty / Success — all four states are explicit. */}
      {isPending && <LoadingProducts />}

      {!isPending && isError && <ProductLoadError retry={() => void refetch()} />}

      {!isPending && !isError && data && data.items.length === 0 && (
        <EmptyProductState />
      )}

      {!isPending && !isError && data && data.items.length > 0 && (
        <>
          <div className="space-y-3" data-testid="product-list">
            {data.items.map((product) => (
              <ProductCard key={product.id} product={product} />
            ))}
          </div>

          <div className="mt-4 flex items-center justify-between">
            <p className="text-sm text-muted-foreground" data-testid="product-count">
              {data.totalCount} {data.totalCount === 1 ? "product" : "products"}
            </p>

            <div className="flex items-center gap-2">
              <Button
                variant="outline"
                disabled={data.page <= 1}
                onClick={() => setPage((current) => current - 1)}
              >
                Previous
              </Button>

              <span className="text-sm text-muted-foreground">
                Page {data.page} of {totalPages}
              </span>

              <Button
                variant="outline"
                disabled={data.page >= totalPages}
                onClick={() => setPage((current) => current + 1)}
              >
                Next
              </Button>
            </div>
          </div>
        </>
      )}
    </>
  );
}
