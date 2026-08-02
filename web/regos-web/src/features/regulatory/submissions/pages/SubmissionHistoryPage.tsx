import { Link, useParams } from "react-router-dom";

import { useCorrespondenceList } from "@/features/regulatory/correspondence/hooks/useCorrespondenceList";

import { useSubmission } from "../hooks/useSubmission";

/**
 * **Two lifecycles, side by side, joined by nobody's aggregate.**
 *
 * The left column is the submission's own history — the steps we are the actor
 * of. The right is what the authority said about it, which arrives as
 * correspondence anchored to this submission (ADR-046).
 *
 * They are composed **here**, on the page, from two projections. The Submission
 * context knows nothing about correspondence and the Interaction context knows
 * nothing about submission status; joining them in either would have bought one
 * screen at the price of a dependency between bounded contexts.
 */
export function SubmissionHistoryPage() {
  const { submissionId } = useParams();

  const { data: submission } = useSubmission(submissionId!);
  const { data: correspondence } = useCorrespondenceList({ submissionId });

  if (!submission) {
    return <div className="p-6 text-muted-foreground">Loading…</div>;
  }

  const inbound = (correspondence ?? []).filter(
    (item) => item.direction === "Inbound",
  );

  return (
    <div className="grid gap-6 p-6 md:grid-cols-2">
      <section className="space-y-3">
        <div>
          <h2 className="text-lg font-semibold">What we did</h2>
          <p className="text-sm text-muted-foreground">
            This submission's own lifecycle.
          </p>
        </div>

        <ol className="divide-y rounded-lg border">
          {submission.history.map((step, index) => (
            <li
              key={`${step.status}-${step.recordedOnUtc}-${index}`}
              className="flex items-baseline justify-between gap-4 p-4"
              data-testid="submission-status-step"
              data-status={step.status}
            >
              <span className="font-medium">{step.status}</span>

              <span className="text-sm text-muted-foreground">
                {step.occurredOn}
              </span>
            </li>
          ))}
        </ol>
      </section>

      <section className="space-y-3">
        <div>
          <h2 className="text-lg font-semibold">What the authority said</h2>
          {/* Not a status on the submission. A letter about it — a fact the
              authority owns and we merely hold (ADR-042). */}
          <p className="text-sm text-muted-foreground">
            Correspondence filed against this sequence.
          </p>
        </div>

        {inbound.length === 0 ? (
          <p
            className="rounded-lg border p-4 text-sm text-muted-foreground"
            data-testid="no-authority-response"
          >
            Nothing yet.
          </p>
        ) : (
          <ol className="divide-y rounded-lg border">
            {inbound.map((item) => (
              <li
                key={item.correspondenceId}
                className="p-4"
                data-testid="authority-response"
              >
                <Link
                  to={`/regulatory/correspondence/${item.correspondenceId}`}
                  className="font-medium hover:underline"
                >
                  {item.subject}
                </Link>

                <p className="text-sm text-muted-foreground">
                  {item.correspondenceTypeName} · {item.occurredOn}
                </p>
              </li>
            ))}
          </ol>
        )}
      </section>
    </div>
  );
}
