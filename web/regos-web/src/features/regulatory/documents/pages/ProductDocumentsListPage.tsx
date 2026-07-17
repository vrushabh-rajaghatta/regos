import { useState } from "react";
import { Link, useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";

import { useProductDocuments } from "../hooks/useProductDocuments";
import { UploadProductDocumentDialog } from "../components/UploadProductDocumentDialog";
import { ProductDocumentStatusBadge } from "../components/ProductDocumentStatusBadge";

export function ProductDocumentsListPage() {
  const { productId } = useParams();

  const [dialogOpen, setDialogOpen] = useState(false);

  const { data, isLoading, error } = useProductDocuments(productId!);

  return (
    <Page>
      <PageHeader
        title="Documents"
        description="Manage regulatory documents for this product."
        actions={
          <Button onClick={() => setDialogOpen(true)}>Upload Document</Button>
        }
      />

      <UploadProductDocumentDialog
        productId={productId!}
        open={dialogOpen}
        onOpenChange={setDialogOpen}
      />

      {isLoading && (
        <p className="text-muted-foreground">Loading Documents...</p>
      )}

      {!isLoading && error && (
        <p className="text-destructive">Failed to load Documents.</p>
      )}

      {!isLoading && !error && data?.length === 0 && (
        <div className="rounded-lg border border-dashed p-12 text-center">
          <h3 className="text-lg font-semibold">
            No documents have been uploaded.
          </h3>

          <p className="mt-2 text-sm text-muted-foreground">
            Upload a file to create the first document for this product.
          </p>

          <Button className="mt-4" onClick={() => setDialogOpen(true)}>
            Upload First Document
          </Button>
        </div>
      )}

      {!isLoading && !error && data && data.length > 0 && (
        <div className="overflow-x-auto rounded-lg border">
          <table className="w-full text-sm">
            <thead className="border-b bg-muted/40 text-left text-muted-foreground">
              <tr>
                <th className="px-4 py-2 font-medium">Name</th>
                <th className="px-4 py-2 font-medium">Document Type</th>
                <th className="px-4 py-2 font-medium">Status</th>
                <th className="px-4 py-2 font-medium">Current Version</th>
                <th className="px-4 py-2 font-medium">Uploaded</th>
                <th className="px-4 py-2 font-medium">Actions</th>
              </tr>
            </thead>

            <tbody>
              {data.map((document) => (
                <tr key={document.id} className="border-b last:border-0">
                  <td className="px-4 py-2 font-medium">{document.name}</td>

                  <td className="px-4 py-2 text-muted-foreground">
                    {document.documentTypeName}
                  </td>

                  <td className="px-4 py-2">
                    <ProductDocumentStatusBadge status={document.status} />
                  </td>

                  <td className="px-4 py-2 text-muted-foreground">
                    {document.currentVersionNumber != null
                      ? `v${document.currentVersionNumber}`
                      : "—"}
                  </td>

                  <td className="px-4 py-2 text-muted-foreground">
                    {new Date(document.createdOnUtc).toLocaleDateString()}
                  </td>

                  <td className="px-4 py-2">
                    <Link
                      to={`/regulatory/products/${productId}/documents/${document.id}`}
                      className="text-primary hover:underline"
                    >
                      Open
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </Page>
  );
}
