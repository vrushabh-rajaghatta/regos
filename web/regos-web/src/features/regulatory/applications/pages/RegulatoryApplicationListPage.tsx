import { useState } from "react";
import { useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";

import { useRegulatoryApplications } from "../hooks/useRegulatoryApplications";
import { RegisterRegulatoryApplicationDialog } from "../components/RegisterRegulatoryApplicationDialog";
import { RegulatoryApplicationCard } from "../components/RegulatoryApplicationCard";

export function RegulatoryApplicationListPage() {
  const { productId } = useParams();

  const [dialogOpen, setDialogOpen] = useState(false);

  const { data, isLoading, error } = useRegulatoryApplications(productId!);

  return (
    <Page>
      <PageHeader
        title="Regulatory Applications"
        description="Manage regulatory applications for this product."
        actions={
          <Button onClick={() => setDialogOpen(true)}>
            New Regulatory Application
          </Button>
        }
      />

      <RegisterRegulatoryApplicationDialog
        productId={productId!}
        open={dialogOpen}
        onOpenChange={setDialogOpen}
      />

      {isLoading && (
        <p className="text-muted-foreground">Loading applications...</p>
      )}

      {!isLoading && error && (
        <p className="text-destructive">Failed to load applications.</p>
      )}

      {!isLoading && !error && data?.length === 0 && (
        <div className="rounded-lg border border-dashed p-12 text-center">
          <h3 className="text-lg font-semibold">No Regulatory Applications</h3>

          <p className="mt-2 text-sm text-muted-foreground">
            Create your first Regulatory Application for this product.
          </p>

          <Button className="mt-4" onClick={() => setDialogOpen(true)}>
            New Regulatory Application
          </Button>
        </div>
      )}

      {!isLoading && !error && data && data.length > 0 && (
        <div className="space-y-3">
          {data.map((application) => (
            <RegulatoryApplicationCard
              key={application.id}
              productId={productId!}
              application={application}
            />
          ))}
        </div>
      )}
    </Page>
  );
}
