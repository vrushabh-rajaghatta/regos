import { useParams } from "react-router-dom";

import { Badge } from "@/components/ui/badge";
import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";
import { PageSection } from "@/shared/components/PageSection";

import { ProductNotFoundError } from "../api/getProduct";
import { useProduct } from "../hooks/useProduct";

export function ProductOverviewPage() {
  const { productId } = useParams();

  const { data: product, isPending, error } = useProduct(productId!);

  // Loading / Not found / Error / Success — all four states are explicit, and
  // a missing product is distinguished from a failed request.
  if (isPending) {
    return <p data-testid="product-loading">Loading product...</p>;
  }

  if (error instanceof ProductNotFoundError) {
    return (
      <p data-testid="product-not-found">
        This product does not exist, or it belongs to another organization.
      </p>
    );
  }

  if (error) {
    return <p data-testid="product-error">Unable to load product.</p>;
  }

  return (
    <Page>
      <PageHeader title={product.name} description={product.code} />

      <PageSection title="Overview">
        <dl className="grid grid-cols-2 gap-6">
          <div>
            <dt className="text-sm text-muted-foreground">Product Code</dt>

            <dd className="font-medium" data-testid="product-code">
              {product.code}
            </dd>
          </div>

          <div>
            <dt className="text-sm text-muted-foreground">Product Type</dt>

            <dd className="font-medium" data-testid="product-type">
              {product.type}
            </dd>
          </div>

          <div>
            <dt className="text-sm text-muted-foreground">Status</dt>

            <dd>
              <Badge data-testid="product-status">{product.status}</Badge>
            </dd>
          </div>
        </dl>
      </PageSection>
    </Page>
  );
}
