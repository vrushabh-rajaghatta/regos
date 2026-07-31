import { useState } from "react";
import { useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { PageHeader } from "@/shared/components/PageHeader";

import { CreateOrganizationDivisionDialog } from "../components/CreateOrganizationDivisionDialog";
import { OrganizationStatusBadge } from "../components/OrganizationStatusBadge";
import { useOrganizationDivisions } from "../hooks/useOrganizationDivisions";

/**
 * How the company is organised internally.
 *
 * Read and create only. Divisions have no deactivate command on the server, so
 * the workspace does not offer one — the UI reflects what the backend can do
 * rather than implying capability it would have to fake.
 */
export function OrganizationDivisionsPage() {
  const { organizationId } = useParams();
  const [createOpen, setCreateOpen] = useState(false);

  const { data: divisions, isPending, error } = useOrganizationDivisions(
    organizationId!,
  );

  return (
    <div className="p-6">
      <PageHeader
        title="Divisions"
        description="Business units within this organization"
        actions={
          <Button onClick={() => setCreateOpen(true)}>Add Division</Button>
        }
      />

      <CreateOrganizationDivisionDialog
        organizationId={organizationId!}
        open={createOpen}
        onOpenChange={setCreateOpen}
      />

      <div className="mt-6">
        {isPending && <p data-testid="divisions-loading">Loading divisions...</p>}

        {error && (
          <p data-testid="divisions-error">Unable to load divisions.</p>
        )}

        {divisions?.length === 0 && (
          <p className="text-muted-foreground" data-testid="divisions-empty">
            No divisions recorded.
          </p>
        )}

        {divisions && divisions.length > 0 && (
          <ul className="divide-y rounded-md border" data-testid="division-list">
            {divisions.map((division) => (
              <li
                key={division.divisionId}
                className="flex items-center justify-between gap-4 px-4 py-3"
                data-testid="division-row"
              >
                <div>
                  <p className="font-medium">
                    {division.name}
                    {division.acronym && (
                      <span className="ml-2 text-sm text-muted-foreground">
                        ({division.acronym})
                      </span>
                    )}
                  </p>

                  <p className="text-sm text-muted-foreground">
                    Established {division.statusDate}
                  </p>
                </div>

                <OrganizationStatusBadge status={division.status} />
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
}
