import { Link } from "react-router-dom";
import type { ProductSummary } from "../types/ProductSummary";

interface Props {
  product: ProductSummary;
}

export function ProductCard({ product }: Props) {
  return (
    <Link
      to={`/regulatory/products/${product.id}`}
      className="block rounded-lg border p-4 hover:bg-muted transition"
    >
      <div className="font-medium">{product.name}</div>

      <div className="text-sm text-muted-foreground">{product.type}</div>
    </Link>
  );
}
