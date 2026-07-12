import { useParams } from "react-router-dom";

import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";
import { PageSection } from "@/shared/components/PageSection";

import { useProduct } from "../hooks/useProduct";

export function ProductOverviewPage() {
  const { productId } = useParams();

  const { data: product, isLoading, error } = useProduct(productId!);

  if (isLoading) {
    return <p>Loading product...</p>;
  }

  if (error) {
    return <p>Unable to load product.</p>;
  }

  if (!product) {
    return <p>Product not found.</p>;
  }

  return (
    <Page>
      <PageHeader title={product.name} description={product.type} />

      <PageSection title="Overview">
        <dl className="grid grid-cols-2 gap-6">
          <div>
            <dt className="text-sm text-muted-foreground">Product Type</dt>

            <dd className="font-medium">{product.type}</dd>
          </div>

          <div>
            <dt className="text-sm text-muted-foreground">Status</dt>

            <dd className="font-medium">{product.status}</dd>
          </div>
        </dl>
      </PageSection>
    </Page>
  );
}
