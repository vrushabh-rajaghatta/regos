import { useState } from "react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useProductDocuments } from "@/features/regulatory/documents/hooks/useProductDocuments";

import { useCoreVersionsForProduct } from "../hooks/useCoreVersionsForProduct";
import { useLocalLabelRevisions } from "../hooks/useLocalLabelRevisions";
import { usePrepareLocalLabelRevision } from "../hooks/usePrepareLocalLabelRevision";
import type {
  LocalLabelRevision,
  LocalLabelRevisionStatus,
} from "../types/GlobalLabel";

import { PublishRevisionDialog } from "./PublishRevisionDialog";

interface LocalLabelRevisionsProps {
  globalProductId: string;
  localLabelId: string;
}

const STATUS_LABEL: Record<LocalLabelRevisionStatus, string> = {
  Draft: "In preparation",
  InForce: "In force",
  Superseded: "Superseded",
};

/**
 * One market's regulatory history for one labelling document — and the two
 * things you can do to the revision being prepared: say what it is, and record
 * that the authority approved it.
 *
 * Every entry stays readable. An approved labelling document is a controlled
 * record, so nothing here offers to overwrite one.
 */
export function LocalLabelRevisions({
  globalProductId,
  localLabelId,
}: LocalLabelRevisionsProps) {
  const [open, setOpen] = useState(false);
  const [publishing, setPublishing] = useState<{
    id: string;
    number: number;
  } | null>(null);

  const { data, isLoading, error } = useLocalLabelRevisions(
    localLabelId,
    open,
  );
  const { data: documents } = useProductDocuments(open ? globalProductId : "");
  const { data: coreVersions } = useCoreVersionsForProduct(
    open ? globalProductId : "",
  );

  const prepare = usePrepareLocalLabelRevision();

  if (!open) {
    return (
      <Button
        variant="ghost"
        size="sm"
        className="mt-2 px-0 text-xs"
        onClick={() => setOpen(true)}
        data-testid="show-local-revisions"
      >
        Show revisions
      </Button>
    );
  }

  const revisions = data ?? [];

  // A restate, not a patch — the whole prepared statement goes each time, so
  // changing the document cannot silently drop the derivation.
  function restate(
    revision: LocalLabelRevision,
    change: Partial<{
      contentId: string | null;
      derivedFromGlobalLabelVersionId: string | null;
    }>,
  ) {
    prepare.mutate({
      localLabelId,
      revisionId: revision.id,
      contentId: revision.contentId,
      derivedFromGlobalLabelVersionId: revision.derivedFromGlobalLabelVersionId,
      dataCarrierCode: revision.dataCarrierCode,
      changeSummary: revision.changeSummary,
      ...change,
    });
  }

  return (
    <div className="mt-3 space-y-2 border-t pt-3">
      {isLoading && (
        <p className="text-xs text-muted-foreground">Loading revisions...</p>
      )}

      {error && (
        <p className="text-xs text-destructive">Failed to load revisions.</p>
      )}

      {prepare.isError && (
        <p className="text-xs text-destructive" data-testid="prepare-error">
          {(prepare.error as Error).message}
        </p>
      )}

      {revisions.map((revision) => (
        <div
          key={revision.id}
          className="rounded-md border p-3 text-sm"
          data-testid="local-revision-row"
        >
          <div className="flex flex-wrap items-center gap-2">
            <span className="font-medium">
              Revision {revision.revisionNumber}
            </span>

            <Badge
              variant={revision.status === "InForce" ? "default" : "outline"}
            >
              {STATUS_LABEL[revision.status]}
            </Badge>

            {revision.derivedFromGlobalLabelVersionNumber !== null && (
              <span className="text-xs text-muted-foreground">
                from core v{revision.derivedFromGlobalLabelVersionNumber}
              </span>
            )}
          </div>

          {revision.approvedOn && (
            <p className="mt-1 text-xs text-muted-foreground">
              Approved {revision.approvedOn} · effective{" "}
              {revision.effectiveFrom}
              {revision.effectiveTo ? ` — ${revision.effectiveTo}` : " onwards"}
            </p>
          )}

          {revision.changeSummary && (
            <p className="mt-1 text-xs text-muted-foreground">
              {revision.changeSummary}
            </p>
          )}

          {revision.status === "Draft" && (
            <div className="mt-2 flex flex-wrap items-center gap-2">
              <Select
                value={revision.contentId ?? ""}
                onValueChange={(contentId) => restate(revision, { contentId })}
              >
                <SelectTrigger
                  className="w-64"
                  data-testid="local-label-content"
                >
                  <SelectValue placeholder="Attach the approved document" />
                </SelectTrigger>

                <SelectContent>
                  {(documents ?? []).map((document) => (
                    <SelectItem key={document.id} value={document.id}>
                      {document.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              {/* Optional on purpose: a migrated portfolio does not know which
                  core version a historical revision came from (D3). */}
              <Select
                value={revision.derivedFromGlobalLabelVersionId ?? ""}
                onValueChange={(derivedFromGlobalLabelVersionId) =>
                  restate(revision, { derivedFromGlobalLabelVersionId })
                }
              >
                <SelectTrigger className="w-64" data-testid="derived-from">
                  <SelectValue placeholder="Derived from (optional)" />
                </SelectTrigger>

                <SelectContent>
                  {(coreVersions ?? []).map((option) => (
                    <SelectItem key={option.id} value={option.id}>
                      {option.labelName} v{option.versionNumber}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <Button
                size="sm"
                onClick={() =>
                  setPublishing({
                    id: revision.id,
                    number: revision.revisionNumber,
                  })
                }
                data-testid="publish-revision"
              >
                Put in force
              </Button>
            </div>
          )}
        </div>
      ))}

      {publishing && (
        <PublishRevisionDialog
          localLabelId={localLabelId}
          revisionId={publishing.id}
          revisionNumber={publishing.number}
          open
          onOpenChange={(next) => !next && setPublishing(null)}
        />
      )}
    </div>
  );
}
