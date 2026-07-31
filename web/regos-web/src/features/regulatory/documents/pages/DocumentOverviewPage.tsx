import { useState, type ReactNode } from "react";
import { useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";

import { useProductDocument } from "../hooks/useProductDocument";
import { useActivateProductDocument } from "../hooks/useActivateProductDocument";
import { useArchiveProductDocument } from "../hooks/useArchiveProductDocument";
import { ProductDocumentStatusBadge } from "../components/ProductDocumentStatusBadge";
import { formatFileSize } from "../utils/formatFileSize";

function Row({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div>
      <div className="text-sm text-muted-foreground">{label}</div>
      <div className="font-medium">{children}</div>
    </div>
  );
}

function LifecycleActions({
  globalProductId,
  documentId,
  status,
}: {
  globalProductId: string;
  documentId: string;
  status: string;
}) {
  const [message, setMessage] = useState<string | null>(null);

  const activate = useActivateProductDocument(globalProductId, documentId);
  const archive = useArchiveProductDocument(globalProductId, documentId);

  const isPending = activate.isPending || archive.isPending;
  const error = activate.error ?? archive.error;

  async function onActivate() {
    setMessage(null);
    await activate.mutateAsync();
    setMessage("Document activated.");
  }

  async function onArchive() {
    setMessage(null);
    await archive.mutateAsync();
    setMessage("Document archived.");
  }

  return (
    <section className="space-y-4 border-t pt-8">
      <h2 className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
        Lifecycle
      </h2>

      {status === "Draft" && (
        <Button onClick={onActivate} disabled={isPending}>
          {activate.isPending ? "Activating..." : "Activate"}
        </Button>
      )}

      {status === "Active" && (
        <Button
          variant="destructive"
          onClick={onArchive}
          disabled={isPending}
        >
          {archive.isPending ? "Archiving..." : "Archive"}
        </Button>
      )}

      {status === "Archived" && (
        <p className="text-sm text-muted-foreground">
          This document is archived. No lifecycle actions are available.
        </p>
      )}

      {error && (
        <p className="text-sm text-destructive">{(error as Error).message}</p>
      )}

      {message && !error && (
        <p className="text-sm text-muted-foreground">{message}</p>
      )}
    </section>
  );
}

export function DocumentOverviewPage() {
  const { globalProductId, documentId } = useParams();

  const {
    data: document,
    isLoading,
    error,
  } = useProductDocument(globalProductId!, documentId!);

  if (isLoading) {
    return (
      <div className="p-6">
        <p className="text-muted-foreground">Loading Document...</p>
      </div>
    );
  }

  if (error || !document) {
    return (
      <div className="p-6">
        <p className="text-destructive">Unable to load Document.</p>
      </div>
    );
  }

  const version = document.currentVersion;

  return (
    <div className="max-w-2xl space-y-8 p-6">
      <section className="space-y-4">
        <h2 className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
          Basic Information
        </h2>

        <Row label="Name">{document.name}</Row>
        <Row label="Document Type">{document.documentTypeName}</Row>

        <div>
          <div className="text-sm text-muted-foreground">Status</div>
          <div className="mt-1">
            <ProductDocumentStatusBadge status={document.status} />
          </div>
        </div>

        <Row label="Product">{document.productName}</Row>
        <Row label="Created On">
          {new Date(document.createdOnUtc).toLocaleString()}
        </Row>
      </section>

      <section className="space-y-4 border-t pt-8">
        <h2 className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
          Current Version
        </h2>

        {version ? (
          <>
            <Row label="Version">v{version.versionNumber}</Row>
            <Row label="Original File Name">{version.originalFileName}</Row>
            <Row label="Content Type">{version.contentType}</Row>
            <Row label="File Size">{formatFileSize(version.fileSize)}</Row>
            <Row label="Uploaded On">
              {new Date(version.uploadedOnUtc).toLocaleString()}
            </Row>
          </>
        ) : (
          <p className="text-muted-foreground">No version uploaded.</p>
        )}
      </section>

      <LifecycleActions
        globalProductId={globalProductId!}
        documentId={documentId!}
        status={document.status}
      />
    </div>
  );
}
