import { Link, useParams } from "react-router-dom";

import { Badge } from "@/components/ui/badge";
import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";
import { PageSection } from "@/shared/components/PageSection";

import { useRegulatoryApplication } from "../hooks/useRegulatoryApplication";

export function ApplicationOverviewPage() {
  const { productId, applicationId } = useParams();

  const {
    data: application,
    isLoading,
    error,
  } = useRegulatoryApplication(productId!, applicationId!);

  if (isLoading) {
    return (
      <Page>
        <p className="text-muted-foreground">Loading application...</p>
      </Page>
    );
  }

  if (error || !application) {
    return (
      <Page>
        <p className="text-destructive">Unable to load application.</p>
      </Page>
    );
  }

  return (
    <Page>
      <PageHeader
        title={application.name}
        description={`${application.countryName} (${application.authorityCode})`}
        actions={<Badge>{application.status}</Badge>}
      />

      <Link
        to={`/regulatory/products/${productId}/applications`}
        className="text-sm text-muted-foreground hover:underline"
      >
        ← Back to Regulatory Applications
      </Link>

      <PageSection title="Overview">
        <dl className="grid grid-cols-2 gap-6">
          <div>
            <dt className="text-sm text-muted-foreground">Application Name</dt>
            <dd className="font-medium">{application.name}</dd>
          </div>

          <div>
            <dt className="text-sm text-muted-foreground">Status</dt>
            <dd className="font-medium">{application.status}</dd>
          </div>

          <div>
            <dt className="text-sm text-muted-foreground">Country</dt>
            <dd className="font-medium">
              {application.countryName} ({application.countryCode})
            </dd>
          </div>

          <div>
            <dt className="text-sm text-muted-foreground">Authority</dt>
            <dd className="font-medium">
              {application.authorityName} ({application.authorityCode})
            </dd>
          </div>

          <div>
            <dt className="text-sm text-muted-foreground">
              Applicant Organization
            </dt>
            <dd className="font-medium">{application.organizationName}</dd>
          </div>
        </dl>
      </PageSection>
    </Page>
  );
}
