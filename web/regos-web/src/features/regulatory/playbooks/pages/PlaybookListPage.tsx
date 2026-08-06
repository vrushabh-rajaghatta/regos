import { Link } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { PageHeader } from "@/shared/components/PageHeader";

import { PlaybookStatusBadge } from "../components/PlaybookStatusBadge";
import { usePlaybooks } from "../hooks/usePlaybooks";

export function PlaybookListPage() {
  const { data, isPending, isError, refetch } = usePlaybooks();

  return (
    <>
      <PageHeader
        title="Playbooks"
        description="What it takes to get something filed, in order — the reusable process a plan is built from."
      />

      <div className="mt-6">
        {isPending && (
          <div className="rounded-lg border p-8 text-center text-muted-foreground">
            Loading playbooks...
          </div>
        )}

        {!isPending && isError && (
          <div className="rounded-lg border border-destructive/40 p-8 text-center">
            <p className="text-sm text-destructive" role="alert">
              Could not load playbooks. Check that the API is running.
            </p>

            <Button variant="outline" className="mt-3" onClick={() => refetch()}>
              Retry
            </Button>
          </div>
        )}

        {!isPending && !isError && data && data.length === 0 && (
          <div className="rounded-lg border border-dashed p-8 text-center text-muted-foreground">
            No playbooks found.
          </div>
        )}

        {!isPending && !isError && data && data.length > 0 && (
          <div className="space-y-3" data-testid="playbook-list">
            {data.map((playbook) => (
              <Link
                key={playbook.id}
                to={playbook.id}
                data-testid="playbook-row"
                className="block rounded-lg border p-4 transition-colors hover:bg-muted"
              >
                <div className="flex items-start justify-between gap-4">
                  <div>
                    <div className="flex items-center gap-2">
                      <span className="font-medium">{playbook.name}</span>

                      {playbook.isShared && (
                        <span className="rounded bg-muted px-1.5 py-0.5 text-xs text-muted-foreground">
                          Provided by RegOS
                        </span>
                      )}
                    </div>

                    <div className="mt-1 text-sm text-muted-foreground">
                      {playbook.countryName} · {playbook.authorityName} ·{" "}
                      {playbook.applicationTypeName}
                    </div>

                    {playbook.description && (
                      <p className="mt-2 max-w-prose text-sm text-muted-foreground">
                        {playbook.description}
                      </p>
                    )}
                  </div>

                  <div className="flex shrink-0 flex-col items-end gap-2">
                    <PlaybookStatusBadge status={playbook.status} />

                    <span className="text-xs text-muted-foreground">
                      {playbook.currentVersionNumber === null
                        ? "Not yet published"
                        : `Version ${playbook.currentVersionNumber}`}
                    </span>
                  </div>
                </div>
              </Link>
            ))}
          </div>
        )}
      </div>
    </>
  );
}
