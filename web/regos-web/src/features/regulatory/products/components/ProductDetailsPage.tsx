import { useParams } from "react-router-dom";

import { useProduct } from "../hooks/useProduct";

export function ProductDetailsPage() {
  const { productId } = useParams();
  const { data, isLoading, error } = useProduct(productId!);

  if (isLoading) {
    return <p>Loading product...</p>;
  }

  if (error) {
    return <p>Unable to load product.</p>;
  }

  if (!data) {
    return <p>Product not found.</p>;
  }

  return (
    <>
      <h1 className="text-3xl font-semibold">{data.name}</h1>
      <div className="mt-6 space-y-2">
        <div>
          <strong>Type:</strong> {data.type}
        </div>
        <div>
          <strong>Status:</strong> {data.status}
        </div>
      </div>
    </>
  );
}
