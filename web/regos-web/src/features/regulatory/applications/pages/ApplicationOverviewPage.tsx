import { useParams } from "react-router-dom";

import { useApplication } from "../hooks/useApplication";
import { ApplicationStatusBadge } from "../components/ApplicationStatusBadge";
import { ApplicationActivity } from "../components/ApplicationActivity";
import { ApplicationDashboardSummary } from "../components/ApplicationDashboardSummary";
import { ApplicationContactsSection } from "@/features/regulatory/submissions/components/ApplicationContactsSection";

export function ApplicationOverviewPage() {
  const { applicationId } = useParams();

  const {
    data: application,
    isLoading,
    error,
  } = useApplication(applicationId!);

  if (isLoading) {
    return (
      <div className="p-6">
        <p className="text-muted-foreground">Loading Application...</p>
      </div>
    );
  }

  if (error || !application) {
    return (
      <div className="p-6">
        <p className="text-destructive">Unable to load Application.</p>
      </div>
    );
  }

  return (
    <div className="max-w-2xl space-y-8 p-6">
      <section className="space-y-4">
        <h2 className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
          Application
        </h2>

        <div>
          <div className="text-sm text-muted-foreground">Name</div>
          <div className="font-medium">{application.name}</div>
        </div>

        <div>
          <div className="text-sm text-muted-foreground">Status</div>
          <div className="mt-1">
            <ApplicationStatusBadge status={application.status} />
          </div>
        </div>
      </section>

      <section className="space-y-4 border-t pt-8">
        <h2 className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
          Jurisdiction
        </h2>

        <div>
          <div className="text-sm text-muted-foreground">Country</div>
          <div className="font-medium">{application.countryName}</div>
        </div>

        <div>
          <div className="text-sm text-muted-foreground">Authority</div>
          <div className="font-medium">{application.authorityName}</div>
        </div>
      </section>

      <section className="space-y-4 border-t pt-8">
        <h2 className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
          Applicant
        </h2>

        <div>
          <div className="text-sm text-muted-foreground">Organization</div>
          <div className="font-medium">
            {application.applicantOrganizationName}
          </div>
        </div>
      </section>

      <section className="space-y-4 border-t pt-8">
        <h2 className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
          Audit
        </h2>

        <div>
          <div className="text-sm text-muted-foreground">Created On</div>
          <div className="font-medium">
            {new Date(application.createdOn).toLocaleString()}
          </div>
        </div>
      </section>

      <section className="space-y-4 border-t pt-8">
        <h2 className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
          Contacts
        </h2>

        {/* Read from the latest published sequence rather than stored on the
            application — see the component (ADR-048). */}
        <ApplicationContactsSection applicationId={applicationId!} />
      </section>

      <section className="space-y-4 border-t pt-8">
        <h2 className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
          Activity
        </h2>

        <ApplicationActivity
          activities={[
            { type: "Created", occurredOn: application.createdOn },
          ]}
        />
      </section>

      <section className="space-y-4 border-t pt-8">
        <h2 className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
          Summary
        </h2>

        <ApplicationDashboardSummary
          items={[
            { title: "Submissions", value: 0 },
            { title: "Documents", value: 0 },
          ]}
        />
      </section>
    </div>
  );
}
