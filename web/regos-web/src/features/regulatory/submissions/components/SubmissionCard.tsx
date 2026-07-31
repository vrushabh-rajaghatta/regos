import { Link } from "react-router-dom";

import type { SubmissionSummary } from "../types/SubmissionSummary";

interface SubmissionCardProps {
  globalProductId: string;
  applicationId: string;
  submission: SubmissionSummary;
}

export function SubmissionCard({
  globalProductId,
  applicationId,
  submission,
}: SubmissionCardProps) {
  // Preserves the full business context in the URL:
  // product -> application -> submission.
  return (
    <Link
      to={`/regulatory/products/${globalProductId}/applications/${applicationId}/submissions/${submission.id}`}
      className="block rounded-lg border p-4 transition-colors hover:bg-muted/50"
    >
      <div className="flex items-start justify-between gap-4">
        <div className="space-y-1">
          <h3 className="font-semibold">{submission.title}</h3>

          <p className="text-sm text-muted-foreground">
            {submission.submissionTypeName}
          </p>
        </div>

        <span className="text-sm text-muted-foreground">
          {submission.status}
        </span>
      </div>
    </Link>
  );
}
