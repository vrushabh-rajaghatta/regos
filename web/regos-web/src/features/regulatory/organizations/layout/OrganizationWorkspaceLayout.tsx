import { Link, Outlet, useParams } from "react-router-dom";

import { OrganizationNotFoundError } from "../api/getOrganization";
import { OrganizationStatusBadge } from "../components/OrganizationStatusBadge";
import { useOrganization } from "../hooks/useOrganization";
import { organizationTypeLabel } from "../types/OrganizationType";
import { OrganizationWorkspaceNavigation } from "./OrganizationWorkspaceNavigation";

/**
 * The workspace shell: which company you are in, and the four angles on it.
 *
 * Not-found is answered here rather than in each of the four pages — a missing
 * organization has no divisions, sites or contacts to fail at separately, and
 * one clear answer beats four.
 */
export function OrganizationWorkspaceLayout() {
  const { organizationId } = useParams();

  const { data: organization, isPending, error } = useOrganization(
    organizationId!,
  );

  if (error instanceof OrganizationNotFoundError) {
    return (
      <div className="space-y-4 p-6">
        <p data-testid="organization-not-found">
          This organization does not exist.
        </p>

        <Link to="/regulatory/organizations" className="text-sm hover:underline">
          Back to organizations
        </Link>
      </div>
    );
  }

  if (error) {
    return (
      <p className="p-6" data-testid="organization-error">
        Unable to load organization.
      </p>
    );
  }

  return (
    <div className="flex h-full">
      <aside className="flex w-64 flex-col border-r bg-muted/20">
        <div className="border-b p-4">
          {isPending ? (
            <p className="text-sm text-muted-foreground">Loading...</p>
          ) : (
            <div className="space-y-1" data-testid="organization-workspace-header">
              <p className="font-medium leading-tight">
                {organization.legalName}
              </p>

              <p className="text-xs text-muted-foreground">
                {organizationTypeLabel(organization.type)}
              </p>

              <OrganizationStatusBadge status={organization.status} />
            </div>
          )}
        </div>

        <OrganizationWorkspaceNavigation />
      </aside>

      <main className="flex-1 overflow-auto">
        {/* Held back until the organization is known, so a child page never
            renders against an id that turns out not to exist. */}
        {!isPending && <Outlet />}
      </main>
    </div>
  );
}
