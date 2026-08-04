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

import { useAttachGlobalLabelContent } from "../hooks/useAttachGlobalLabelContent";
import { useGlobalLabelVersions } from "../hooks/useGlobalLabelVersions";
import type { GlobalLabelVersionStatus } from "../types/GlobalLabel";

import { PublishVersionDialog } from "./PublishVersionDialog";

interface GlobalLabelVersionsProps {
  globalProductId: string;
  globalLabelId: string;
}

const STATUS_LABEL: Record<GlobalLabelVersionStatus, string> = {
  Draft: "Draft",
  InForce: "In force",
  Superseded: "Superseded",
};

/**
 * Every issue of one label, and the two things you can do to the open draft:
 * point it at a document, and put it in force.
 *
 * Opened per label rather than always fetched — a product with four labels
 * should not load four histories to render a list.
 */
export function GlobalLabelVersions({
  globalProductId,
  globalLabelId,
}: GlobalLabelVersionsProps) {
  const [open, setOpen] = useState(false);
  const [publishing, setPublishing] = useState<{
    id: string;
    number: number;
  } | null>(null);

  const { data, isLoading, error } = useGlobalLabelVersions(globalLabelId, open);
  const { data: documents } = useProductDocuments(open ? globalProductId : "");

  const attach = useAttachGlobalLabelContent();

  if (!open) {
    return (
      <Button
        variant="ghost"
        size="sm"
        className="mt-2 px-0 text-xs"
        onClick={() => setOpen(true)}
        data-testid="show-label-versions"
      >
        Show versions
      </Button>
    );
  }

  const versions = data ?? [];

  return (
    <div className="mt-3 space-y-2 border-t pt-3">
      {isLoading && (
        <p className="text-xs text-muted-foreground">Loading versions...</p>
      )}

      {error && (
        <p className="text-xs text-destructive">Failed to load versions.</p>
      )}

      {/* A failed attach and a list that has not refreshed look identical
          otherwise — SC-106. */}
      {attach.isError && (
        <p className="text-xs text-destructive" data-testid="attach-error">
          {(attach.error as Error).message}
        </p>
      )}

      {versions.map((version) => (
        <div
          key={version.id}
          className="rounded-md border p-3 text-sm"
          data-testid="label-version-row"
        >
          <div className="flex flex-wrap items-center gap-2">
            <span className="font-medium">Version {version.versionNumber}</span>

            <Badge
              variant={version.status === "InForce" ? "default" : "outline"}
            >
              {STATUS_LABEL[version.status]}
            </Badge>

            {version.effectiveFrom && (
              <span className="text-xs text-muted-foreground">
                {version.effectiveFrom}
                {version.effectiveTo ? ` — ${version.effectiveTo}` : " onwards"}
              </span>
            )}
          </div>

          {version.changeSummary && (
            <p className="mt-1 text-xs text-muted-foreground">
              {version.changeSummary}
            </p>
          )}

          {version.status === "Draft" && (
            <div className="mt-2 flex flex-wrap items-center gap-2">
              {/* The content link. A picker over this product's documents, so
                  the "belongs to another product" refusal is a race guard
                  rather than the everyday path (ADR-059 §6). */}
              {/* Controlled from the first render, with `""` for "nothing
                  attached" — never `undefined`. The server owns which document
                  a version points at, and a value that starts undefined and
                  becomes a string switches the Select from uncontrolled to
                  controlled mid-life. That is a real defect, not console
                  noise: the component's state model is decided on first render
                  and changing it mid-life makes the displayed value stop
                  tracking the server's. */}
              <Select
                value={version.contentId ?? ""}
                onValueChange={(contentId) => {
                  // The Select signals "cleared" with null. Nothing here offers
                  // that gesture, and detaching a document from a version is not
                  // a capability — publishing requires content, so a version
                  // that lost its document would be a state the domain refuses.
                  if (!contentId) return;

                  attach.mutate({
                    globalLabelId,
                    versionId: version.id,
                    contentId,
                  });
                }}
              >
                <SelectTrigger className="w-72" data-testid="label-content">
                  <SelectValue placeholder="Attach the label document" />
                </SelectTrigger>

                <SelectContent>
                  {(documents ?? []).map((document) => (
                    <SelectItem key={document.id} value={document.id}>
                      {document.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              <Button
                size="sm"
                onClick={() =>
                  setPublishing({
                    id: version.id,
                    number: version.versionNumber,
                  })
                }
                data-testid="publish-version"
              >
                Publish
              </Button>
            </div>
          )}
        </div>
      ))}

      {publishing && (
        <PublishVersionDialog
          globalLabelId={globalLabelId}
          versionId={publishing.id}
          versionNumber={publishing.number}
          open
          onOpenChange={(next) => !next && setPublishing(null)}
        />
      )}
    </div>
  );
}
