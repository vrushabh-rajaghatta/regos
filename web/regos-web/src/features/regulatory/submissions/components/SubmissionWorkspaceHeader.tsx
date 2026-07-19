import { useParams } from "react-router-dom";

import { useSubmission } from "../hooks/useSubmission";
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
            {submission.submissionTypeName}
          </p>
        </div>

        <SubmissionStatusBadge status={submission.status} />
      </div>
    </header>
  );
}
