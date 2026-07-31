import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { useAttachableProductDocuments } from "../hooks/useAttachableProductDocuments";
import { useAttachProductDocument } from "../hooks/useAttachProductDocument";
import { usePlaceSubmissionDocument } from "../hooks/usePlaceSubmissionDocument";
import type { ContentPlanDocument } from "../types/SubmissionContentPlan";

interface Props {
  submissionId: string;
  /** The section being filled — null while the dialog is closed. */
  section: { id: string; label: string } | null;
  /** Attached to the submission but sitting nowhere yet. */
  unplaced: ContentPlanDocument[];
  onOpenChange: (open: boolean) => void;
}

/**
 * Puts a document into one section of the dossier.
 *
 * Two routes to the same outcome, because a user filling a gap does not care
 * which: a document already attached is *moved* into the section, and one that
 * is not is attached and placed in a single call. The distinction is
 * bookkeeping, so the dialog keeps it out of the user's way while still
 * labelling which is which.
 */
export function PlaceDocumentDialog({
  submissionId,
  section,
  unplaced,
  onOpenChange,
}: Props) {
  const open = section !== null;

  const attachable = useAttachableProductDocuments(submissionId, open);
  const attach = useAttachProductDocument(submissionId);
  const place = usePlaceSubmissionDocument(submissionId);

  const busy = attach.isPending || place.isPending;
  const error = (attach.error ?? place.error) as Error | undefined;

  const done = () => onOpenChange(false);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-2xl">
        <DialogHeader>
          <DialogTitle>Place a document in {section?.label}</DialogTitle>
        </DialogHeader>

        {unplaced.length > 0 && (
          <section>
            <h3 className="text-sm font-medium">Already attached</h3>
            <p className="mb-2 text-sm text-muted-foreground">
              Attached to this submission but not yet placed anywhere.
            </p>

            <ul className="divide-y rounded-lg border">
              {unplaced.map((document) => (
                <li
                  key={document.submissionDocumentId}
                  className="flex items-center justify-between gap-4 px-4 py-2"
                >
                  <span>
                    <span className="font-medium">{document.name}</span>{" "}
                    <span className="text-sm text-muted-foreground">
                      {document.documentTypeName}
                    </span>
                  </span>

                  <Button
                    size="sm"
                    disabled={busy}
                    data-testid="place-attached-document"
                    onClick={() =>
                      place.mutate(
                        {
                          submissionDocumentId: document.submissionDocumentId,
                          templateSectionId: section!.id,
                        },
                        { onSuccess: done }
                      )
                    }
                  >
                    Place here
                  </Button>
                </li>
              ))}
            </ul>
          </section>
        )}

        <section>
          <h3 className="text-sm font-medium">Product Documents</h3>
          <p className="mb-2 text-sm text-muted-foreground">
            Active documents for this product, attached and placed in one step.
          </p>

          {attachable.isLoading && (
            <p className="text-sm text-muted-foreground">Loading documents...</p>
          )}

          {!attachable.isLoading && attachable.error && (
            <p className="text-sm text-destructive">
              Failed to load available documents.
            </p>
          )}

          {!attachable.isLoading &&
            !attachable.error &&
            attachable.data?.length === 0 && (
              <div className="rounded-lg border border-dashed p-6 text-center text-sm text-muted-foreground">
                No active documents are available to attach.
              </div>
            )}

          {!attachable.isLoading &&
            !attachable.error &&
            attachable.data &&
            attachable.data.length > 0 && (
              <ul className="max-h-72 divide-y overflow-y-auto rounded-lg border">
                {attachable.data.map((document) => (
                  <li
                    key={document.productDocumentId}
                    className="flex items-center justify-between gap-4 px-4 py-2"
                  >
                    <span>
                      <span className="font-medium">{document.name}</span>{" "}
                      <span className="text-sm text-muted-foreground">
                        {document.documentType}
                      </span>
                    </span>

                    <Button
                      size="sm"
                      variant="outline"
                      disabled={busy}
                      data-testid="attach-and-place-document"
                      onClick={() =>
                        attach.mutate(
                          {
                            productDocumentId: document.productDocumentId,
                            templateSectionId: section!.id,
                          },
                          { onSuccess: done }
                        )
                      }
                    >
                      Attach and place
                    </Button>
                  </li>
                ))}
              </ul>
            )}
        </section>

        {error && <p className="text-sm text-destructive">{error.message}</p>}

        <div className="flex justify-end">
          <Button variant="outline" onClick={done}>
            Cancel
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
