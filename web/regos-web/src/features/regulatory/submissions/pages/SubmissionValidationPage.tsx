import { useParams } from "react-router-dom";

import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";

import { useSubmissionValidation } from "../hooks/useSubmissionValidation";
import type {
  ValidationIssue,
  ValidationSeverity,
} from "../types/SubmissionValidation";

// Rendered in this order always. Publishability is decided by errors alone;
// warnings advise and information explains.
const GROUPS: { severity: ValidationSeverity; label: string; tone: string }[] = [
  { severity: "Error", label: "Errors", tone: "text-destructive" },
  { severity: "Warning", label: "Warnings", tone: "text-amber-600" },
  {
    severity: "Information",
    label: "Information",
    tone: "text-muted-foreground",
  },
];

export function SubmissionValidationPage() {
  const { submissionId } = useParams();

  const { data, isLoading, error } = useSubmissionValidation(submissionId!);

  const issues = data?.issues ?? [];
  const errorCount = issues.filter((i) => i.severity === "Error").length;

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

      {!isLoading && !error && data && (
        <div className="space-y-6">
          {/*
            Publishability and findings are separate questions. A submission can
            be ready to publish and still have things worth knowing — advisory
            rules, or checks this validator cannot perform yet — so the summary
            never claims there is nothing to report.
          */}
          {data.isValid ? (
            <div
              className="rounded-lg border border-emerald-600/40 bg-emerald-50 p-6 dark:bg-emerald-950/30"
              data-testid="validation-status"
              data-valid="true"
            >
              <h3 className="text-lg font-semibold text-emerald-700 dark:text-emerald-400">
                Ready to publish
              </h3>
              <p className="mt-1 text-sm text-muted-foreground">
                Nothing blocks publishing this submission.
                {issues.length > 0 && " See the notes below before you do."}
              </p>
            </div>
          ) : (
            <div
              className="rounded-lg border border-destructive/40 p-6"
              data-testid="validation-status"
              data-valid="false"
            >
              <h3 className="text-lg font-semibold text-destructive">
                {errorCount} blocking {errorCount === 1 ? "issue" : "issues"}
              </h3>
              <p className="mt-1 text-sm text-muted-foreground">
                Resolve these before this submission can be published.
              </p>
            </div>
          )}

          {GROUPS.map(({ severity, label, tone }) => {
            const group = issues.filter((i) => i.severity === severity);

            if (group.length === 0) return null;

            return (
              <section
                key={severity}
                className="space-y-2"
                data-testid="validation-group"
                data-severity={severity}
              >
                <h4 className={`text-sm font-semibold uppercase ${tone}`}>
                  {label} ({group.length})
                </h4>

                <ul className="divide-y rounded-lg border">
                  {group.map((issue, index) => (
                    <IssueRow
                      key={`${issue.code}-${issue.ruleCode ?? ""}-${index}`}
                      issue={issue}
                    />
                  ))}
                </ul>
              </section>
            );
          })}
        </div>
      )}
    </Page>
  );
}

function IssueRow({ issue }: { issue: ValidationIssue }) {
  return (
    <li className="px-4 py-3" data-testid="validation-issue">
      <p className="text-sm">{issue.message}</p>

      <div className="mt-1 flex flex-wrap items-center gap-2">
        <span className="text-xs text-muted-foreground">{issue.code}</span>

        {/* Traceability back to the regulatory rule that produced this. */}
        {issue.ruleCode && (
          <span
            className="rounded bg-muted px-1.5 py-0.5 font-mono text-xs"
            data-testid="issue-rule-code"
          >
            {issue.ruleCode}
          </span>
        )}
      </div>

      {/* Structured, so this never depends on the wording of the message. */}
      {issue.unevaluatedRuleTypes && issue.unevaluatedRuleTypes.length > 0 && (
        <ul className="mt-2 flex flex-wrap gap-2">
          {issue.unevaluatedRuleTypes.map((ruleType) => (
            <li
              key={ruleType}
              className="rounded bg-muted px-1.5 py-0.5 font-mono text-xs"
              data-testid="unevaluated-rule-type"
            >
              {ruleType}
            </li>
          ))}
        </ul>
      )}
    </li>
  );
}
