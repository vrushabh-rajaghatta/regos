import { useState } from "react";
import { useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { PageHeader } from "@/shared/components/PageHeader";

import { useRegulatoryApplications } from "../hooks/useRegulatoryApplications";
import { RegisterRegulatoryApplicationDialog } from "../components/RegisterRegulatoryApplicationDialog";

export function RegulatoryApplicationListPage() {
  const { productId } = useParams();

  const [dialogOpen, setDialogOpen] = useState(false);

  const {
    data,
    isLoading,
    error,
  } = useRegulatoryApplications(productId!);

  return (
    <>
      <PageHeader
        title="Applications"
        description="Manage regulatory applications for this product."
        actions={
          <Button onClick={() => setDialogOpen(true)}>
            New Application
          </Button>
        }
      />

      <RegisterRegulatoryApplicationDialog
        productId={productId!}
        open={dialogOpen}
        onOpenChange={setDialogOpen}
      />

      {isLoading && (
        <p className="text-muted-foreground">
          Loading applications...
        </p>
      )}

      {!isLoading && error && (
        <p className="text-destructive">
          Failed to load applications.
        </p>
      )}

      {!isLoading && !error && data?.length === 0 && (
        <div className="rounded-lg border border-dashed p-12 text-center">
          <h3 className="text-lg font-semibold">
            No applications yet
          </h3>

          <p className="mt-2 text-sm text-muted-foreground">
            Create the first regulatory application for this product.
          </p>
        </div>
      )}

      {!isLoading && !error && data && data.length > 0 && (
        <div className="space-y-3">
          {data.map((application) => (
            <div
              key={application.id}
              className="rounded-lg border p-4"
            >
              <div className="flex items-center justify-between">
                <div>
                  <h3 className="font-semibold">
                    {application.name}
                  </h3>

                  <p className="text-sm text-muted-foreground">
                    {application.applicationNumber ?? "No application number"}
                  </p>
                </div>

                <span className="text-sm">
                  {application.status}
                </span>
              </div>
            </div>
          ))}
        </div>
      )}
    </>
  );
}
