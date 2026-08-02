import { useParams } from "react-router-dom";

import { useSubmission } from "../hooks/useSubmission";
import { SubmissionStatusBadge } from "../components/SubmissionStatusBadge";
import { SubmissionFormatField } from "../components/SubmissionFormatField";

export function SubmissionOverviewPage() {
  const { submissionId } = useParams();

  const {
    data: submission,
    isLoading,
    error,
  } = useSubmission(submissionId!);

  if (isLoading) {
    return (
      <div className="p-6">
        <p className="text-muted-foreground">Loading Submission...</p>
      </div>
    );
  }

  if (error || !submission) {
    return (
      <div className="p-6">
        <p className="text-destructive">Unable to load Submission.</p>
      </div>
    );
  }

  return (
    <div className="max-w-2xl space-y-8 p-6">
      <section className="space-y-4">
        <h2 className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
          Submission
        </h2>

        <div>
          <div className="text-sm text-muted-foreground">Title</div>
          <div className="font-medium">{submission.title}</div>
        </div>

        <div>
          <div className="text-sm text-muted-foreground">Status</div>
          <div className="mt-1">
            <SubmissionStatusBadge status={submission.status} />
          </div>
        </div>
      </section>

      <section className="space-y-4 border-t pt-8">
        <h2 className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
          Type
        </h2>

        <div>
          <div className="text-sm text-muted-foreground">Submission Type</div>
          <div className="font-medium">{submission.submissionTypeName}</div>
        </div>

        <SubmissionFormatField
          submissionId={submission.id}
          format={submission.format}
          status={submission.status}
        />
      </section>

      <section className="space-y-4 border-t pt-8">
        <h2 className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
          Application
        </h2>

        <div>
          <div className="text-sm text-muted-foreground">Application Name</div>
          <div className="font-medium">{submission.applicationName}</div>
        </div>
      </section>

      <section className="space-y-4 border-t pt-8">
        <h2 className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
          Audit
        </h2>

        <div>
          <div className="text-sm text-muted-foreground">Created On</div>
          <div className="font-medium">
            {new Date(submission.createdOn).toLocaleString()}
          </div>
        </div>
      </section>
    </div>
  );
}
