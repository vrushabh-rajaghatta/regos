import { Link } from "react-router-dom";

import type { SubmissionSummary } from "../types/SubmissionSummary";
import { sequenceLabel } from "../utils/sequenceLabel";

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

        <div className="flex flex-col items-end gap-1">
          <span className="text-sm text-muted-foreground">
            {submission.status}
          </span>

          {/* Only what was actually filed. A draft carries no number here —
              the expectation belongs on the submission's own page, where
              someone is about to act on it. */}
          {submission.sequenceNumber !== null && (
            <span className="font-mono text-sm">
              {sequenceLabel(submission.sequenceNumber)}
            </span>
          )}
        </div>
      </div>
    </Link>
  );
}
