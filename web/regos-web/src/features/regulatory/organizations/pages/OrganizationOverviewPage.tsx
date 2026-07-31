import { useState } from "react";
import { Link, useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { PageHeader } from "@/shared/components/PageHeader";
import { PageSection } from "@/shared/components/PageSection";

import { ActivateOrganizationDialog } from "../components/ActivateOrganizationDialog";
import { DeactivateOrganizationDialog } from "../components/DeactivateOrganizationDialog";
import { EditOrganizationDialog } from "../components/EditOrganizationDialog";
import { OrganizationIdentifiers } from "../components/OrganizationIdentifiers";
import { OrganizationStatusBadge } from "../components/OrganizationStatusBadge";
import { useOrganization } from "../hooks/useOrganization";
import { organizationTypeLabel } from "../types/OrganizationType";

/**
 * Who this company is: the identity it carries and the registries that know it.
 *
 * Not-found and load errors are the workspace layout's job — by the time this
 * renders, the organization exists.
 */
export function OrganizationOverviewPage() {
  const { organizationId } = useParams();
  const [editOpen, setEditOpen] = useState(false);
  const [deactivateOpen, setDeactivateOpen] = useState(false);
  const [activateOpen, setActivateOpen] = useState(false);

  const { data: organization } = useOrganization(organizationId!);

  if (!organization) return null;

  return (
    <div className="p-6">
      <PageHeader
        title={organization.legalName}
        description={organizationTypeLabel(organization.type)}
        actions={
          <div className="flex items-center gap-3">
            <Link
              to="/regulatory/organizations"
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
              <dd className="mt-1 flex items-center gap-2">
                <OrganizationStatusBadge status={organization.status} />

                <span className="text-sm text-muted-foreground">
                  since {organization.statusDate}
                </span>
              </dd>
            </div>

            {/* Only when recorded. An empty row for a company that files in one
                script would be noise dressed as information. */}
            {organization.acronym && (
              <div>
                <dt className="text-sm text-muted-foreground">Acronym</dt>
                <dd className="mt-1 font-medium">{organization.acronym}</dd>
              </div>
            )}

            {organization.nameNativeLanguage && (
              <div>
                <dt className="text-sm text-muted-foreground">
                  Name (Native Language)
                </dt>
                <dd className="mt-1 font-medium">
                  {organization.nameNativeLanguage}
                </dd>
              </div>
            )}
          </dl>
        </PageSection>
      </div>

      <div className="mt-6">
        <PageSection title="Identifiers">
          <OrganizationIdentifiers
            organizationId={organization.id}
            identifiers={organization.identifiers}
          />
        </PageSection>
      </div>
    </div>
  );
}
