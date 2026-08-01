import { useRef, useState } from "react";

import { Button } from "@/components/ui/button";
import { buildUrl } from "@/shared/api/apiClient";

import { useAttachCorrespondenceContent } from "../hooks/useAttachCorrespondenceContent";
import { useRemoveCorrespondenceContent } from "../hooks/useRemoveCorrespondenceContent";
import type { CorrespondenceAttachmentSummary } from "../types/CorrespondenceDetail";
import { formatFileSize } from "../utils/formatFileSize";

interface CorrespondenceContentProps {
  correspondenceId: string;
  attachments: CorrespondenceAttachmentSummary[];
}

/**
 * The letter's own content.
 *
 * Deliberately not called "Documents": a `ProductDocument` is a governed
 * business object with a CTD type, an approval lifecycle and versions, and
 * none of that applies to a PDF someone posted us. Removing a file here leaves
 * the correspondence untouched — the record is the letter, the file is its
 * content (ADR-040 decision 5).
 */
export function CorrespondenceContent({
  correspondenceId,
  attachments,
}: CorrespondenceContentProps) {
  const fileInput = useRef<HTMLInputElement>(null);
  const [pendingRemoval, setPendingRemoval] = useState<string | null>(null);

  const attach = useAttachCorrespondenceContent(correspondenceId);
  const remove = useRemoveCorrespondenceContent(correspondenceId);

  async function onFileChosen(event: React.ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];
    if (!file) return;

    try {
      await attach.mutateAsync(file);
    } catch {
      // A refusal is an outcome, not a crash; the reason renders below.
      return;
    } finally {
      if (fileInput.current) fileInput.current.value = "";
    }
  }

  async function onRemove(attachmentId: string) {
    setPendingRemoval(attachmentId);

    try {
      await remove.mutateAsync(attachmentId);
    } catch {
      return;
    } finally {
      setPendingRemoval(null);
    }
  }

  return (
    <section className="mt-6 rounded-lg border p-6" data-testid="correspondence-content">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h2 className="text-xl font-semibold">Content</h2>
          <p className="mt-1 text-sm text-muted-foreground">
            The letter itself, and anything sent with it.
          </p>
        </div>

        <div>
          <input
            ref={fileInput}
            type="file"
            aria-label="Choose a file to attach"
            className="hidden"
            onChange={onFileChosen}
          />

          <Button
            type="button"
            disabled={attach.isPending}
            onClick={() => fileInput.current?.click()}
          >
            {attach.isPending ? "Attaching..." : "Attach file"}
          </Button>
        </div>
      </div>

      {(attach.isError || remove.isError) && (
        <p
          className="mt-4 text-sm text-destructive"
          data-testid="correspondence-content-error"
        >
          {((attach.error ?? remove.error) as Error).message}
        </p>
      )}

      {attachments.length === 0 ? (
        <p
          className="mt-4 text-sm text-muted-foreground"
          data-testid="correspondence-content-empty"
        >
          Nothing attached yet.
        </p>
      ) : (
        <ul className="mt-4 divide-y" data-testid="correspondence-content-list">
          {attachments.map((attachment) => (
            <li
              key={attachment.attachmentId}
              className="flex items-center justify-between gap-4 py-3"
            >
              <div>
                <a
                  className="font-medium underline-offset-4 hover:underline"
                  href={buildUrl(
                    `/api/correspondence/${correspondenceId}/content/${attachment.attachmentId}`,
                  )}
                >
                  {attachment.originalFileName}
                </a>
                <p className="text-sm text-muted-foreground">
                  {formatFileSize(attachment.fileSizeBytes)}
                </p>
              </div>

              <Button
                type="button"
                variant="outline"
                disabled={pendingRemoval === attachment.attachmentId}
                onClick={() => onRemove(attachment.attachmentId)}
              >
                Remove
              </Button>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
