import { useState } from "react";
import { Link, useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { PageHeader } from "@/shared/components/PageHeader";
import { PageSection } from "@/shared/components/PageSection";

import { OrganizationNotFoundError } from "../api/getOrganization";
import { ActivateOrganizationDialog } from "../components/ActivateOrganizationDialog";
import { DeactivateOrganizationDialog } from "../components/DeactivateOrganizationDialog";
import { EditOrganizationDialog } from "../components/EditOrganizationDialog";
import { OrganizationStatusBadge } from "../components/OrganizationStatusBadge";
import { useOrganization } from "../hooks/useOrganization";
import { organizationTypeLabel } from "../types/OrganizationType";

export function OrganizationDetailsPage() {
  const { organizationId } = useParams();
  const [editOpen, setEditOpen] = useState(false);
  const [deactivateOpen, setDeactivateOpen] = useState(false);
  const [activateOpen, setActivateOpen] = useState(false);

  const { data: organization, isPending, error } = useOrganization(
    organizationId!,
  );

  // Loading / Not found / Error / Success are all explicit, and a missing
  // organization is distinguished from a failed request.
  if (isPending) {
    return (
      <p data-testid="organization-loading">Loading organization...</p>
    );
  }

  if (error instanceof OrganizationNotFoundError) {
    return (
      <div className="space-y-4">
        <p data-testid="organization-not-found">
          This organization does not exist.
        </p>

        <Link
          to="/platform/organizations"
          className="text-sm hover:underline"
        >
          Back to organizations
        </Link>
      </div>
    );
  }

  if (error) {
    return (
      <p data-testid="organization-error">Unable to load organization.</p>
    );
  }

  return (
    <>
      <PageHeader
        title={organization.legalName}
        description={organizationTypeLabel(organization.type)}
        actions={
          <div className="flex items-center gap-3">
            <Link
              to="/platform/organizations"
              className="text-sm hover:underline"
            >
              Back
            </Link>

            <Button variant="outline" onClick={() => setEditOpen(true)}>
              Edit
            </Button>

            {/* Exactly one lifecycle action is offered, because exactly one
                transition is legal. The other would answer 409. */}
            {organization.status === "Active" ? (
              <Button onClick={() => setDeactivateOpen(true)}>
                Deactivate
              </Button>
            ) : (
              <Button onClick={() => setActivateOpen(true)}>Activate</Button>
            )}
          </div>
        }
      />

      <EditOrganizationDialog
        organization={organization}
        open={editOpen}
        onOpenChange={setEditOpen}
      />

      <DeactivateOrganizationDialog
        organizationId={organization.id}
        legalName={organization.legalName}
        open={deactivateOpen}
        onOpenChange={setDeactivateOpen}
      />

      <ActivateOrganizationDialog
        organizationId={organization.id}
        legalName={organization.legalName}
        open={activateOpen}
        onOpenChange={setActivateOpen}
      />

      <div className="mt-6">
        <PageSection title="Details">
          <dl
            className="grid grid-cols-1 gap-4 sm:grid-cols-3"
            data-testid="organization-details"
          >
            <div>
              <dt className="text-sm text-muted-foreground">Legal Name</dt>
              <dd className="mt-1 font-medium">{organization.legalName}</dd>
            </div>

            <div>
              <dt className="text-sm text-muted-foreground">Type</dt>
              <dd className="mt-1 font-medium">
                {organizationTypeLabel(organization.type)}
              </dd>
            </div>

            <div>
              <dt className="text-sm text-muted-foreground">Status</dt>
              <dd className="mt-1">
                <OrganizationStatusBadge status={organization.status} />
              </dd>
            </div>
          </dl>
        </PageSection>
      </div>
    </>
  );
}
