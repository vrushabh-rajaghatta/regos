import { useState } from "react";

import { Button } from "@/components/ui/button";
import { PageHeader } from "@/shared/components/PageHeader";

import { CreateOrganizationDialog } from "../components/CreateOrganizationDialog";
import { OrganizationsTable } from "../components/OrganizationsTable";
import { useOrganizationDirectory } from "../hooks/useOrganizationDirectory";

export function OrganizationsPage() {
  const [dialogOpen, setDialogOpen] = useState(false);

  const { data, isPending, isError, refetch } = useOrganizationDirectory();

  return (
    <>
      <PageHeader
        title="Organizations"
        description="Create and review the organizations on the platform."
        actions={
          <Button onClick={() => setDialogOpen(true)}>
            Create Organization
          </Button>
        }
      />

      <CreateOrganizationDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
      />

      <div className="mt-6">
        {/* Loading */}
        {isPending && (
          <div className="rounded-lg border p-8 text-center text-muted-foreground">
            Loading organizations...
          </div>
        )}

        {/* Error */}
        {!isPending && isError && (
          <div className="rounded-lg border border-destructive/40 p-8 text-center">
            <p className="text-sm text-destructive" role="alert">
              Could not load organizations. Check that the API is running.
            </p>

            <Button
              variant="outline"
              className="mt-3"
              onClick={() => refetch()}
            >
              Retry
            </Button>
          </div>
        )}

        {/* Empty */}
        {!isPending && !isError && data && data.length === 0 && (
          <div className="rounded-lg border border-dashed p-8 text-center text-muted-foreground">
            No organizations found. Create your first organization to get
            started.
          </div>
        )}

        {/* Success */}
        {!isPending && !isError && data && data.length > 0 && (
          <>
            <OrganizationsTable organizations={data} />

            <div
              className="mt-3 text-sm text-muted-foreground"
              data-testid="organization-count"
            >
              {data.length} organization{data.length === 1 ? "" : "s"}
            </div>
          </>
        )}
      </div>
    </>
  );
}
