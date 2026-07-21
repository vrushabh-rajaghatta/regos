import { useState } from "react";
import { Link } from "react-router-dom";

import { Button } from "@/components/ui/button";

import type { OrganizationListItem } from "../types/OrganizationListItem";
import { organizationTypeLabel } from "../types/OrganizationType";
import { DeactivateOrganizationDialog } from "./DeactivateOrganizationDialog";
import { OrganizationStatusBadge } from "./OrganizationStatusBadge";

interface OrganizationsTableProps {
  organizations: OrganizationListItem[];
}

export function OrganizationsTable({
  organizations,
}: OrganizationsTableProps) {
  const [deactivating, setDeactivating] =
    useState<OrganizationListItem | null>(null);

  return (
    <>
      <div className="overflow-x-auto rounded-lg border">
        <table className="w-full text-sm" data-testid="organization-list">
          <thead className="border-b bg-muted/50">
            <tr className="text-left">
              <th className="px-4 py-2.5 font-medium">Legal Name</th>
              <th className="px-4 py-2.5 font-medium">Type</th>
              <th className="px-4 py-2.5 font-medium">Status</th>
              <th className="px-4 py-2.5 font-medium text-right">Actions</th>
            </tr>
          </thead>

          <tbody>
            {organizations.map((organization) => (
              <tr
                key={organization.id}
                className="border-b last:border-0 hover:bg-muted/40"
              >
                <td className="px-4 py-2.5 font-medium">
                  <Link
                    to={`/platform/organizations/${organization.id}`}
                    className="hover:underline"
                  >
                    {organization.legalName}
                  </Link>
                </td>

                <td className="px-4 py-2.5 text-muted-foreground">
                  {organizationTypeLabel(organization.type)}
                </td>

                <td className="px-4 py-2.5" data-testid="organization-status">
                  <OrganizationStatusBadge status={organization.status} />
                </td>

                <td className="px-4 py-2.5 text-right">
                  {/* An inactive organization has no deactivate action: the
                      API answers 409, so offering the button would invite a
                      failure the UI already knows about. */}
                  {organization.status === "Active" && (
                    <Button
                      variant="outline"
                      size="sm"
                      aria-label={`Deactivate ${organization.legalName}`}
                      onClick={() => setDeactivating(organization)}
                    >
                      Deactivate
                    </Button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {deactivating && (
        <DeactivateOrganizationDialog
          organizationId={deactivating.id}
          legalName={deactivating.legalName}
          open={true}
          onOpenChange={(open) => {
            if (!open) setDeactivating(null);
          }}
        />
      )}
    </>
  );
}
