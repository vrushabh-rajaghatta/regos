import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { useAttachableProductDocuments } from "../hooks/useAttachableProductDocuments";
import { useAttachProductDocument } from "../hooks/useAttachProductDocument";

interface Props {
  submissionId: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function AttachProductDocumentDialog({
  submissionId,
  open,
  onOpenChange,
}: Props) {
  const { data, isLoading, error } = useAttachableProductDocuments(
    submissionId,
    open
  );

  const attach = useAttachProductDocument(submissionId);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-2xl">
        <DialogHeader>
          <DialogTitle>Attach Product Document</DialogTitle>
        </DialogHeader>

        <p className="text-sm text-muted-foreground">
          Active documents for this product that are not yet attached.
        </p>

        {isLoading && (
          <p className="text-sm text-muted-foreground">Loading documents...</p>
        )}

        {!isLoading && error && (
          <p className="text-sm text-destructive">
            Failed to load available documents.
          </p>
        )}

        {!isLoading && !error && data?.length === 0 && (
          <div className="rounded-lg border border-dashed p-8 text-center text-sm text-muted-foreground">
            No active documents are available to attach.
          </div>
        )}

        {!isLoading && !error && data && data.length > 0 && (
          <div className="max-h-96 overflow-y-auto rounded-lg border">
            <table className="w-full text-sm">
              <thead className="border-b bg-muted/40 text-left text-muted-foreground">
                <tr>
                  <th className="px-4 py-2 font-medium">Name</th>
                  <th className="px-4 py-2 font-medium">Type</th>
                  <th className="px-4 py-2 font-medium">Version</th>
                  <th className="px-4 py-2 font-medium">Status</th>
                  <th className="px-4 py-2 font-medium">Updated</th>
                  <th className="px-4 py-2" />
                </tr>
              </thead>

              <tbody>
                {data.map((doc) => {
                  const pending =
                    attach.isPending &&
                    attach.variables?.productDocumentId ===
                      doc.productDocumentId;

                  return (
                    <tr
                      key={doc.productDocumentId}
                      className="border-b last:border-0"
                    >
                      <td className="px-4 py-2 font-medium">{doc.name}</td>
                      <td className="px-4 py-2 text-muted-foreground">
                        {doc.documentType}
                      </td>
                      <td className="px-4 py-2 text-muted-foreground">
                        {doc.currentVersionNumber != null
                          ? `v${doc.currentVersionNumber}`
                          : "—"}
                      </td>
                      <td className="px-4 py-2 text-muted-foreground">
                        {doc.status}
                      </td>
                      <td className="px-4 py-2 text-muted-foreground">
                        {new Date(doc.createdOnUtc).toLocaleDateString()}
                      </td>
                      <td className="px-4 py-2 text-right">
                        <Button
                          size="sm"
                          variant="outline"
                          disabled={attach.isPending}
                          onClick={() =>
                            attach.mutate({
                              productDocumentId: doc.productDocumentId,
                            })
                          }
                          data-testid="attach-document"
                        >
                          {pending ? "Attaching..." : "Attach"}
                        </Button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}

        {attach.isError && (
          <p className="text-sm text-destructive">
            {(attach.error as Error).message}
          </p>
        )}

        <div className="flex justify-end">
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Done
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
