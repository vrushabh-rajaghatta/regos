import { useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";

import { SubmissionStatusBadge } from "../components/SubmissionStatusBadge";
import { useSubmission } from "../hooks/useSubmission";
import { useSubmissionValidation } from "../hooks/useSubmissionValidation";
import { usePublishSubmission } from "../hooks/usePublishSubmission";

export function SubmissionPublishingPage() {
  const { submissionId } = useParams();

  const submission = useSubmission(submissionId!);
  const validation = useSubmissionValidation(submissionId!);
  const publish = usePublishSubmission(submissionId!);

  const status = submission.data?.status;
  const isDraft = status === "Draft";
  const isPublished = status === "Published";
  const isReady = validation.data?.isValid === true;

  // Reasons from the most recent refused publish, if any.
  const refusedIssues =
    publish.data && !publish.data.published
      ? publish.data.validation?.issues ?? []
      : [];

  return (
    <Page>
      <PageHeader
        title="Publishing"
        description="Finalize this submission. Once published it can no longer be modified."
      />

      <div className="flex items-center gap-2">
        <span className="text-sm text-muted-foreground">Status</span>
        {status && <SubmissionStatusBadge status={status} />}
      </div>

      {isPublished && (
        <div className="rounded-lg border border-emerald-600/40 bg-emerald-50 p-6 dark:bg-emerald-950/30">
          <h3 className="text-lg font-semibold text-emerald-700 dark:text-emerald-400">
            Published
          </h3>
          <p className="mt-1 text-sm text-muted-foreground">
            This submission has been finalized and is now immutable.
          </p>
        </div>
      )}

      {isDraft && (
        <div className="space-y-4">
          {validation.isLoading && (
            <p className="text-muted-foreground">Checking readiness...</p>
          )}

          {!validation.isLoading && isReady && (
            <p className="text-sm text-emerald-700 dark:text-emerald-400">
              This submission is ready to publish.
            </p>
          )}

          {!validation.isLoading && validation.data && !isReady && (
            <p className="text-sm text-muted-foreground">
              This submission has open validation issues and cannot be published
              yet. See the Validation tab for details.
            </p>
          )}

          <Button
            disabled={!isReady || publish.isPending}
            onClick={() => publish.mutate()}
          >
            {publish.isPending ? "Publishing..." : "Publish"}
          </Button>

          {refusedIssues.length > 0 && (
            <div className="space-y-2">
              <p className="text-sm font-medium text-destructive">
                Publishing was refused:
              </p>
              <ul className="divide-y rounded-lg border">
                {refusedIssues.map((issue, index) => (
                  <li key={`${issue.code}-${index}`} className="px-4 py-2 text-sm">
                    {issue.message}
                  </li>
                ))}
              </ul>
            </div>
          )}

          {publish.isError && (
            <p className="text-sm text-destructive">
              {(publish.error as Error).message}
            </p>
          )}
        </div>
      )}
    </Page>
  );
}
