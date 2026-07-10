import { useProducts } from "../hooks/useProducts";

export function ProductListPage() {
  const { data, isLoading, error } = useProducts();

  if (isLoading) {
    return <p>Loading products...</p>;
  }

  if (error) {
    return <p>Unable to load products.</p>;
  }

  return (
    <div>
      <h1 className="text-2xl font-semibold mb-6">Products</h1>

      <ul className="space-y-3">
        {data?.map((product) => (
          <li key={product.id} className="border rounded-lg p-4">
            <div className="font-medium">{product.name}</div>
            <div className="text-sm text-muted-foreground">
              {product.type} • {product.status}
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}
