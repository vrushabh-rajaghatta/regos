import { useState } from "react";
import { useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";

import { useSubmission } from "../hooks/useSubmission";
import { useSubmissionDocuments } from "../hooks/useSubmissionDocuments";
import { useRemoveProductDocument } from "../hooks/useRemoveProductDocument";
import { AttachProductDocumentDialog } from "../components/AttachProductDocumentDialog";

export function SubmissionDocumentsPage() {
  const { submissionId } = useParams();

  const [dialogOpen, setDialogOpen] = useState(false);

  const submission = useSubmission(submissionId!);
  const { data, isLoading, error } = useSubmissionDocuments(submissionId!);
  const remove = useRemoveProductDocument(submissionId!);

  // Only a draft dossier may change — mirrors the aggregate invariant.
  const isDraft = submission.data?.status === "Draft";

  return (
    <Page>
      <PageHeader
        title="Documents"
        description="The Product Documents that make up this submission dossier."
        actions={
          isDraft ? (
            <Button onClick={() => setDialogOpen(true)}>
              Attach Product Document
            </Button>
          ) : undefined
        }
      />

      {isDraft && (
        <AttachProductDocumentDialog
          submissionId={submissionId!}
          open={dialogOpen}
          onOpenChange={setDialogOpen}
        />
      )}

      {!isDraft && submission.data && (
        <p className="text-sm text-muted-foreground">
          This submission is no longer a draft. Its documents are read-only.
        </p>
      )}

      {isLoading && (
        <p className="text-muted-foreground">Loading documents...</p>
      )}

      {!isLoading && error && (
        <p className="text-destructive">Failed to load documents.</p>
      )}

      {!isLoading && !error && data?.length === 0 && (
        <div className="rounded-lg border border-dashed p-12 text-center">
          <h3 className="text-lg font-semibold">
            No documents have been attached to this submission.
          </h3>

          {isDraft && (
            <Button className="mt-4" onClick={() => setDialogOpen(true)}>
              Attach Product Document
            </Button>
          )}
        </div>
      )}

      {!isLoading && !error && data && data.length > 0 && (
        <div className="overflow-x-auto rounded-lg border">
          <table className="w-full text-sm">
            <thead className="border-b bg-muted/40 text-left text-muted-foreground">
              <tr>
                <th className="px-4 py-2 font-medium">#</th>
                <th className="px-4 py-2 font-medium">Document</th>
                <th className="px-4 py-2 font-medium">Type</th>
                <th className="px-4 py-2 font-medium">Version</th>
                <th className="px-4 py-2 font-medium">Attached On</th>
                {isDraft && <th className="px-4 py-2 font-medium">Actions</th>}
              </tr>
            </thead>

            <tbody>
              {data.map((doc) => (
                <tr
                  key={doc.submissionDocumentId}
                  className="border-b last:border-0"
                >
                  <td className="px-4 py-2 text-muted-foreground">
                    {doc.displayOrder}
                  </td>
                  <td className="px-4 py-2 font-medium">{doc.documentName}</td>
                  <td className="px-4 py-2 text-muted-foreground">
                    {doc.documentType}
                  </td>
                  <td className="px-4 py-2 text-muted-foreground">
                    v{doc.versionNumber}
                  </td>
                  <td className="px-4 py-2 text-muted-foreground">
                    {new Date(doc.attachedOnUtc).toLocaleDateString()}
                  </td>
                  {isDraft && (
                    <td className="px-4 py-2">
                      <Button
                        size="sm"
                        variant="destructive"
                        disabled={remove.isPending}
                        onClick={() =>
                          remove.mutate(doc.submissionDocumentId)
                        }
                      >
                        Remove
                      </Button>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {remove.isError && (
        <p className="text-sm text-destructive">
          {(remove.error as Error).message}
        </p>
      )}
    </Page>
  );
}
