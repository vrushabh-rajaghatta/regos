import { Link } from "react-router-dom";

import { Badge } from "@/components/ui/badge";

import type { ProductSummary } from "../types/ProductSummary";

interface ProductCardProps {
  product: ProductSummary;
}

export function ProductCard({ product }: ProductCardProps) {
  return (
    <Link to={`/regulatory/products/${product.id}`} className="block">
      <div className="flex items-start justify-between">
        <div className="space-y-1">
          <h3 className="text-lg font-semibold">{product.name}</h3>

          <p className="text-sm text-muted-foreground">
            {product.code} · {product.type}
          </p>
        </div>

        <Badge>{product.status}</Badge>
      </div>
    </Link>
  );
}
