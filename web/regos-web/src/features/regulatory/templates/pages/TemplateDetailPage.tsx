import { Link, useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { useDocumentTypes } from "@/features/regulatory/documents/hooks/useDocumentTypes";
import { PageHeader } from "@/shared/components/PageHeader";

import { BlueprintTree } from "../components/BlueprintTree";
import { TemplateStatusBadge } from "../components/TemplateStatusBadge";
import { useRegulatoryTemplate } from "../hooks/useRegulatoryTemplate";

export function TemplateDetailPage() {
  const { templateId } = useParams();

  const { data, isPending, isError, refetch } =
    useRegulatoryTemplate(templateId);
  const { data: documentTypes } = useDocumentTypes();

  const documentTypeName = (id: string) =>
    documentTypes?.find((type) => type.id === id)?.name ?? id;

  // Show the published version; fall back to the latest one if none is
  // published yet (draft-only templates aren't seeded today, but the read
  // model can carry them).
  const version = data
    ? (data.versions.find((v) => v.status === "Published") ??
      data.versions.at(-1))
    : undefined;

  return (
    <>
      <Link
        to="/regulatory/templates"
        className="text-sm text-muted-foreground hover:underline"
      >
        ← Regulatory Templates
      </Link>

      <div className="mt-2">
        {isPending && (
          <div className="rounded-lg border p-8 text-center text-muted-foreground">
            Loading template...
          </div>
        )}

        {!isPending && isError && (
          <div className="rounded-lg border border-destructive/40 p-8 text-center">
            <p className="text-sm text-destructive" role="alert">
              Could not load this template. Check that the API is running.
            </p>

            <Button variant="outline" className="mt-3" onClick={() => refetch()}>
              Retry
            </Button>
          </div>
        )}

        {!isPending && !isError && data && (
          <>
            <PageHeader
              title={data.name}
              description={`${data.code} · ${data.source}`}
              actions={<TemplateStatusBadge status={data.status} />}
            />

            {version ? (
              <div className="mt-6 space-y-4">
                <div className="flex flex-wrap items-center gap-2 text-sm text-muted-foreground">
                  <span>Version {version.versionNumber}</span>
                  <TemplateStatusBadge status={version.status} />
                  {version.effectiveFrom && (
                    <span>· Effective {version.effectiveFrom}</span>
                  )}
                </div>

                <BlueprintTree
                  version={version}
                  documentTypeName={documentTypeName}
                />
              </div>
            ) : (
              <div className="mt-6 rounded-lg border border-dashed p-8 text-center text-muted-foreground">
                This template has no versions yet.
              </div>
            )}
          </>
        )}
      </div>
    </>
  );
}
