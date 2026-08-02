import { Link, useParams } from "react-router-dom";

import { formatLabel } from "@/features/regulatory/submissions/utils/formatLabel";
import {
  nextSequenceLabel,
  sequenceLabel,
} from "@/features/regulatory/submissions/utils/sequenceLabel";

import { useProductDocumentUsage } from "../hooks/useProductDocumentUsage";
import type { DocumentUsageItem } from "../types/DocumentUsageItem";

/**
 * **One document's life across an application's filings** — EPIC-004 S006.
 *
 * The capstone read, and it stores nothing. Every column comes from a decision
 * an earlier story made: the number from S001, the operation from S002, the
 * status from S003, the format from S004. Withdrawals appear beside placements
 * because the read reunifies what the write kept apart (ADR-045).
 */
export function DocumentUsagePage() {
  const { globalProductId, documentId } = useParams();

  const { data, isLoading, error } = useProductDocumentUsage(
    globalProductId!,
    documentId!
  );

  const byApplication = groupByApplication(data ?? []);

  return (
    <div className="space-y-6 p-6">
      <div>
        <h1 className="text-2xl font-semibold">In filings</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Every sequence that placed this document, and every sequence that
          withdrew it — in the order they were filed.
        </p>
      </div>

      {isLoading && <p className="text-muted-foreground">Loading filings...</p>}

      {!isLoading && error && (
        <p className="text-destructive">Failed to load this document's filings.</p>
      )}

      {!isLoading && !error && byApplication.length === 0 && (
        <div className="rounded-lg border border-dashed p-12 text-center">
          <p className="text-muted-foreground" data-testid="no-filing-history">
            This document has never been placed in a submission.
          </p>
        </div>
      )}

      {byApplication.map(([applicationName, events]) => (
        <section
          key={applicationName}
          className="space-y-3"
          data-testid="filing-history-application"
        >
          {/* Grouped by application because a sequence number only means
              anything inside one — 0001 in two applications is two filings
              (ADR-044). */}
          <h2 className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
            {applicationName}
          </h2>

          <div className="overflow-x-auto rounded-lg border">
            <table className="w-full text-sm">
              <thead className="border-b bg-muted/40 text-left text-muted-foreground">
                <tr>
                  <th className="px-4 py-2 font-medium">Sequence</th>
                  <th className="px-4 py-2 font-medium">What happened</th>
                  <th className="px-4 py-2 font-medium">Version</th>
                  <th className="px-4 py-2 font-medium">Format</th>
                  <th className="px-4 py-2 font-medium">Submission</th>
                  <th className="px-4 py-2" />
                </tr>
              </thead>

              <tbody>
                {events.map((event, index) => (
                  <tr
                    key={`${event.submissionId}-${event.operation}-${index}`}
                    className="border-b last:border-0"
                    data-testid="filing-history-event"
                    data-operation={event.operation ?? "Draft"}
                    data-sequence={event.sequenceNumber ?? ""}
                  >
                    <td className="px-4 py-2 font-mono">
                      {event.sequenceNumber !== null ? (
                        sequenceLabel(event.sequenceNumber)
                      ) : (
                        <span className="font-sans text-muted-foreground">
                          Draft
                        </span>
                      )}
                    </td>

                    <td className="px-4 py-2">
                      <OperationLabel operation={event.operation} />
                    </td>

                    <td className="px-4 py-2 text-muted-foreground">
                      {/* Null exactly when the document was withdrawn: there is
                          no version, because nothing was placed. */}
                      {event.versionNumber !== null
                        ? `v${event.versionNumber}`
                        : "—"}
                    </td>

                    <td className="px-4 py-2 text-muted-foreground">
                      {formatLabel(event.format)}
                    </td>

                    <td className="px-4 py-2 text-muted-foreground">
                      {event.submissionTitle}
                    </td>

                    <td className="px-4 py-2 text-right">
                      <Link
                        to={`/regulatory/products/${globalProductId}/applications/${event.applicationId}/submissions/${event.submissionId}`}
                        className="text-primary hover:underline"
                      >
                        Open
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <NextSequenceNote events={events} />
        </section>
      ))}
    </div>
  );
}

/**
 * The operation in the words a regulatory user would use. `Unchanged` is not
 * an eCTD operation — it is RegOS's record that a cumulative filing carried the
 * document forward untouched, and saying so is the point of keeping it.
 */
function OperationLabel({ operation }: { operation: string | null }) {
  const words: Record<string, string> = {
    New: "First filed",
    Replace: "Replaced with a newer version",
    Unchanged: "Carried forward unchanged",
    Delete: "Withdrawn",
    Append: "Appended",
  };

  if (operation === null) {
    // A draft has not filed anything, so it has done nothing to the document.
    return <span className="text-muted-foreground">Not yet filed</span>;
  }

  return <span>{words[operation] ?? operation}</span>;
}

/** What the next filing in this application would be numbered. */
function NextSequenceNote({ events }: { events: DocumentUsageItem[] }) {
  const published = events
    .map((event) => event.sequenceNumber)
    .filter((n): n is number => n !== null);

  if (published.length === 0) {
    return null;
  }

  return (
    <p className="text-sm text-muted-foreground">
      {nextSequenceLabel(Math.max(...published) + 1)}.
    </p>
  );
}

/**
 * Preserves the server's ordering — filing order within each application, and
 * applications in the order they first appear.
 */
function groupByApplication(
  events: DocumentUsageItem[]
): [string, DocumentUsageItem[]][] {
  const groups = new Map<string, DocumentUsageItem[]>();

  for (const event of events) {
    const existing = groups.get(event.applicationName);

    if (existing) {
      existing.push(event);
    } else {
      groups.set(event.applicationName, [event]);
    }
  }

  return [...groups.entries()];
}
