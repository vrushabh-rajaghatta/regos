import { useParams } from "react-router-dom";

import { useSubmission } from "../hooks/useSubmission";
import { formatLabel } from "../utils/formatLabel";
import { nextSequenceLabel, sequenceLabel } from "../utils/sequenceLabel";
import { SubmissionStatusBadge } from "./SubmissionStatusBadge";

export function SubmissionWorkspaceHeader() {
  const { submissionId } = useParams();

  // Shares the Overview page's query; React Query serves the cached result,
  // so this does not trigger an additional network request.
  const { data: submission } = useSubmission(submissionId!);

  if (!submission) {
    return null;
  }

  return (
    <header className="border-b px-6 py-4">
      <div className="flex items-start justify-between gap-4">
        <div className="space-y-1">
          <h1 className="text-xl font-semibold">{submission.title}</h1>

          <p className="text-sm text-muted-foreground">
            {submission.submissionTypeName} &middot;{" "}
            <span data-testid="header-format">
              {formatLabel(submission.format)}
            </span>
          </p>
        </div>

        <div className="flex flex-col items-end gap-1">
          <SubmissionStatusBadge status={submission.status} />

          {/* A fact once filed, an expectation before. The two are worded
              differently on purpose: a draft that says "0004" is claiming
              something it has not earned (ADR-044 decision 4). */}
          {submission.sequenceNumber !== null ? (
            <span className="font-mono text-sm">
              {sequenceLabel(submission.sequenceNumber)}
            </span>
          ) : (
            <span className="text-sm text-muted-foreground">
              {nextSequenceLabel(submission.nextSequenceNumber)}
            </span>
          )}
        </div>
      </div>
    </header>
  );
}
