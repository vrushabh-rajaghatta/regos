import { Link } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { PageHeader } from "@/shared/components/PageHeader";

import { TemplateStatusBadge } from "../components/TemplateStatusBadge";
import { useRegulatoryTemplates } from "../hooks/useRegulatoryTemplates";

export function TemplateListPage() {
  const { data, isPending, isError, refetch } = useRegulatoryTemplates();

  return (
    <>
      <PageHeader
        title="Regulatory Templates"
        description="Browse the governed dossier blueprints — the structure, required documents and validation rules a submission of each type must satisfy."
      />

      <div className="mt-6">
        {isPending && (
          <div className="rounded-lg border p-8 text-center text-muted-foreground">
            Loading templates...
          </div>
        )}

        {!isPending && isError && (
          <div className="rounded-lg border border-destructive/40 p-8 text-center">
            <p className="text-sm text-destructive" role="alert">
              Could not load regulatory templates. Check that the API is
              running.
            </p>

            <Button variant="outline" className="mt-3" onClick={() => refetch()}>
              Retry
            </Button>
          </div>
        )}

        {!isPending && !isError && data && data.length === 0 && (
          <div className="rounded-lg border border-dashed p-8 text-center text-muted-foreground">
            No regulatory templates found.
          </div>
        )}

        {!isPending && !isError && data && data.length > 0 && (
          <>
            <div className="space-y-3" data-testid="template-list">
              {data.map((template) => (
                <Link
                  key={template.id}
                  to={template.id}
                  data-testid="template-row"
                  className="block rounded-lg border p-4 transition-colors hover:bg-muted"
                >
                  <div className="flex items-start justify-between gap-4">
                    <div>
                      <h3 className="font-medium">{template.name}</h3>
                      <p className="mt-1 text-sm text-muted-foreground">
                        <span className="font-mono">{template.code}</span>
                        {" · "}
                        {template.source}
                      </p>
                    </div>

                    <TemplateStatusBadge status={template.status} />
                  </div>
                </Link>
              ))}
            </div>

            <div className="mt-3 text-sm text-muted-foreground">
              {data.length} template{data.length === 1 ? "" : "s"}
            </div>
          </>
        )}
      </div>
    </>
  );
}
