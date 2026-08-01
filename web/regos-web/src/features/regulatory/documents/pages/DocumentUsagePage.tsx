import { Link, useParams } from "react-router-dom";

import { useProductDocumentUsage } from "../hooks/useProductDocumentUsage";

export function DocumentUsagePage() {
  const { globalProductId, documentId } = useParams();

  const { data, isLoading, error } = useProductDocumentUsage(
    globalProductId!,
    documentId!
  );

  return (
    <div className="space-y-6 p-6">
      <div>
        <h1 className="text-2xl font-semibold">Usage</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Submissions that reference this document, with the version each one
          pinned.
        </p>
      </div>

      {isLoading && (
        <p className="text-muted-foreground">Loading usage...</p>
      )}

      {!isLoading && error && (
        <p className="text-destructive">Failed to load usage.</p>
      )}

      {!isLoading && !error && data?.length === 0 && (
        <div className="rounded-lg border border-dashed p-12 text-center">
          <p className="text-muted-foreground">
            This document is not currently used by any submissions.
          </p>
        </div>
      )}

      {!isLoading && !error && data && data.length > 0 && (
        <div className="overflow-x-auto rounded-lg border">
          <table className="w-full text-sm">
            <thead className="border-b bg-muted/40 text-left text-muted-foreground">
              <tr>
                <th className="px-4 py-2 font-medium">Submission</th>
                <th className="px-4 py-2 font-medium">Submission Type</th>
                <th className="px-4 py-2 font-medium">Authority</th>
                <th className="px-4 py-2 font-medium">Attached Version</th>
                <th className="px-4 py-2 font-medium">Attached On</th>
                <th className="px-4 py-2" />
              </tr>
            </thead>

            <tbody>
              {data.map((usage) => (
                <tr key={usage.submissionId} className="border-b last:border-0">
                  <td className="px-4 py-2 font-medium">
                    {usage.submissionTitle}
                  </td>
                  <td className="px-4 py-2 text-muted-foreground">
                    {usage.submissionType}
                  </td>
                  <td className="px-4 py-2 text-muted-foreground">
                    {usage.authority}
                  </td>
                  <td className="px-4 py-2 text-muted-foreground">
                    v{usage.versionNumber}
                  </td>
                  <td className="px-4 py-2 text-muted-foreground">
                    {new Date(usage.attachedOnUtc).toLocaleDateString()}
                  </td>
                  <td className="px-4 py-2 text-right">
                    <Link
                      to={`/regulatory/products/${globalProductId}/applications/${usage.applicationId}/submissions/${usage.submissionId}`}
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
    </div>
  );
}
