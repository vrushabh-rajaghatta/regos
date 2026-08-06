import { useState } from "react";
import { Link, useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { PageHeader } from "@/shared/components/PageHeader";

import { PlaybookStatusBadge } from "../components/PlaybookStatusBadge";
import { PlaybookStepTable } from "../components/PlaybookStepTable";
import { usePlaybook } from "../hooks/usePlaybook";

export function PlaybookDetailPage() {
  const { playbookId } = useParams();

  // Undefined asks the server for the version a new plan would be instantiated
  // from — the same resolution a plan will use, rather than a second copy of it
  // written here.
  const [version, setVersion] = useState<number | undefined>(undefined);

  const { data, isPending, isError, refetch } = usePlaybook(playbookId, version);

  return (
    <>
      <Link
        to="/regulatory/playbooks"
        className="text-sm text-muted-foreground hover:underline"
      >
        ← Playbooks
      </Link>

      <div className="mt-2">
        {isPending && (
          <div className="rounded-lg border p-8 text-center text-muted-foreground">
            Loading playbook...
          </div>
        )}

        {!isPending && isError && (
          <div className="rounded-lg border border-destructive/40 p-8 text-center">
            <p className="text-sm text-destructive" role="alert">
              Could not load this playbook.
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
              description={
                data.description ??
                `${data.countryName} · ${data.authorityName} · ${data.applicationTypeName}`
              }
            />

            <div className="mt-4 flex flex-wrap items-center gap-2">
              <PlaybookStatusBadge status={data.status} />

              <span className="font-mono text-xs text-muted-foreground">
                {data.code}
              </span>

              {data.isShared && (
                <span className="rounded bg-muted px-1.5 py-0.5 text-xs text-muted-foreground">
                  Provided by RegOS
                </span>
              )}
            </div>

            <section className="mt-8">
              <h2 className="text-sm font-medium">Versions</h2>

              <p className="mt-1 max-w-prose text-sm text-muted-foreground">
                A published version never changes. A plan created from one stays
                on it for good, so a later version cannot move a milestone that
                has already been scheduled.
              </p>

              <div className="mt-3 flex flex-wrap gap-2" data-testid="playbook-versions">
                {data.versions.map((entry) => (
                  <Button
                    key={entry.id}
                    variant={
                      entry.versionNumber === data.selectedVersionNumber
                        ? "default"
                        : "outline"
                    }
                    size="sm"
                    data-testid="playbook-version-button"
                    onClick={() => setVersion(entry.versionNumber)}
                  >
                    v{entry.versionNumber}
                    <span className="ml-2 text-xs opacity-70">
                      {entry.status}
                    </span>
                  </Button>
                ))}
              </div>
            </section>

            <section className="mt-8">
              <div className="flex items-baseline justify-between gap-4">
                <h2 className="text-sm font-medium">
                  Steps
                  {data.selectedVersionNumber !== null &&
                    ` — version ${data.selectedVersionNumber}`}
                </h2>

                <span className="text-xs text-muted-foreground">
                  {data.steps.length} steps
                </span>
              </div>

              <div className="mt-3">
                {data.steps.length === 0 ? (
                  <div className="rounded-lg border border-dashed p-8 text-center text-muted-foreground">
                    This version has no steps yet.
                  </div>
                ) : (
                  <PlaybookStepTable steps={data.steps} />
                )}
              </div>
            </section>
          </>
        )}
      </div>
    </>
  );
}
