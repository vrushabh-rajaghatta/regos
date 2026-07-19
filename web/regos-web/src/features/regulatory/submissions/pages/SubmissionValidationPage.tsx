import { useParams } from "react-router-dom";

import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";

import { useSubmissionValidation } from "../hooks/useSubmissionValidation";
import type { ValidationSeverity } from "../types/SubmissionValidation";

// Severity drives the accent colour of each issue row.
const severityClass: Record<ValidationSeverity, string> = {
  Error: "text-destructive",
  Warning: "text-amber-600",
  Information: "text-muted-foreground",
};

export function SubmissionValidationPage() {
  const { submissionId } = useParams();

  const { data, isLoading, error } = useSubmissionValidation(submissionId!);

  return (
    <Page>
      <PageHeader
        title="Validation"
        description="Whether this submission is ready to publish, and if not, why."
      />

      {isLoading && (
        <p className="text-muted-foreground">Checking readiness...</p>
      )}

      {!isLoading && error && (
        <p className="text-destructive">Failed to load validation.</p>
      )}

      {!isLoading && !error && data?.isValid && (
        <div className="rounded-lg border border-emerald-600/40 bg-emerald-50 p-6 dark:bg-emerald-950/30">
          <h3 className="text-lg font-semibold text-emerald-700 dark:text-emerald-400">
            Ready to publish
          </h3>
          <p className="mt-1 text-sm text-muted-foreground">
            This submission has no outstanding issues.
          </p>
        </div>
      )}

      {!isLoading && !error && data && !data.isValid && (
        <div className="space-y-3">
          <h3 className="text-lg font-semibold text-destructive">
            {data.issues.length}{" "}
            {data.issues.length === 1 ? "issue" : "issues"} found
          </h3>

          <ul className="divide-y rounded-lg border">
            {data.issues.map((issue, index) => (
              <li
                key={`${issue.code}-${index}`}
                className="flex items-start gap-3 px-4 py-3"
              >
                <span
                  className={`mt-0.5 text-xs font-semibold uppercase ${
                    severityClass[issue.severity]
                  }`}
                >
                  {issue.severity}
                </span>
                <div>
                  <p className="text-sm">{issue.message}</p>
                  <p className="text-xs text-muted-foreground">{issue.code}</p>
                </div>
              </li>
            ))}
          </ul>
        </div>
      )}
    </Page>
  );
}
