import { Link, useParams } from "react-router-dom";

import { useProductDocument } from "../hooks/useProductDocument";
import { ProductDocumentStatusBadge } from "./ProductDocumentStatusBadge";

export function DocumentWorkspaceHeader() {
  const { globalProductId, documentId } = useParams();

  const { data: document } = useProductDocument(globalProductId!, documentId!);

  if (!document) {
    return null;
  }

  return (
    <header className="space-y-2 border-b px-6 py-4">
      {/* Products > {Product} > Documents > {Document} */}
      <nav className="text-sm text-muted-foreground">
        <Link to="/regulatory/products" className="hover:underline">
          Products
        </Link>
        <span className="mx-1">›</span>
        <Link
          to={`/regulatory/products/${globalProductId}`}
          className="hover:underline"
        >
          {document.productName}
        </Link>
        <span className="mx-1">›</span>
        <Link
          to={`/regulatory/products/${globalProductId}/documents`}
          className="hover:underline"
        >
          Documents
        </Link>
        <span className="mx-1">›</span>
        <span className="text-foreground">{document.name}</span>
      </nav>

      <div className="flex items-start justify-between gap-4">
        <div className="space-y-1">
          <h1 className="text-xl font-semibold">{document.name}</h1>

          <p className="text-sm text-muted-foreground">
            {document.documentTypeName}
          </p>
        </div>

        <ProductDocumentStatusBadge status={document.status} />
      </div>
    </header>
  );
}
