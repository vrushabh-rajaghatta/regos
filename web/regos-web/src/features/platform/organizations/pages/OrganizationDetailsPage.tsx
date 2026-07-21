import { Link, useParams } from "react-router-dom";

import { PageHeader } from "@/shared/components/PageHeader";
import { PageSection } from "@/shared/components/PageSection";

import { OrganizationNotFoundError } from "../api/getOrganization";
import { OrganizationStatusBadge } from "../components/OrganizationStatusBadge";
import { useOrganization } from "../hooks/useOrganization";
import { organizationTypeLabel } from "../types/OrganizationType";

export function OrganizationDetailsPage() {
  const { organizationId } = useParams();

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
          <Link
            to="/platform/organizations"
            className="text-sm hover:underline"
          >
            Back to organizations
          </Link>
        }
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
