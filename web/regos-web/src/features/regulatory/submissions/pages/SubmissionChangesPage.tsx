import { useParams } from "react-router-dom";

import { useSubmission } from "../hooks/useSubmission";
import { useSubmissionChanges } from "../hooks/useSubmissionChanges";
import { sequenceLabel } from "../utils/sequenceLabel";

/**
 * What this filing did to the sequence before it.
 *
 * Reads back the operations frozen at publish — nothing is recomputed here. A
 * view that re-derived the diff would answer with today's rule rather than the
 * one the filing was made under (ADR-045).
 */
export function SubmissionChangesPage() {
  const { submissionId } = useParams();
  const { data: submission } = useSubmission(submissionId!);
  const { data, isPending } = useSubmissionChanges(submissionId!);

  if (isPending || !data) {
    return <div className="p-6 text-muted-foreground">Loading…</div>;
  }

  // A draft has changed nothing, because it has filed nothing.
  if (data.sequenceNumber === null) {
    return (
      <div className="p-6">
        <h1 className="text-2xl font-semibold">What changed</h1>
        <p className="mt-2 text-muted-foreground" data-testid="changes-draft">
          Nothing yet. A sequence records what it changed at the moment it is
          published{submission ? ` as ${sequenceLabel(
            submission.nextSequenceNumber
          ).toLowerCase()}` : ""}.
        </p>
      </div>
    );
  }

  const against =
    data.previousSequenceNumber === null
      ? "the first filing in this application"
      : sequenceLabel(data.previousSequenceNumber);

  return (
    <div className="space-y-4 p-6">
      <div>
        <h1 className="text-2xl font-semibold">What changed</h1>
        <p className="mt-1 text-sm text-muted-foreground" data-testid="changes-baseline">
          {sequenceLabel(data.sequenceNumber)}, measured against {against}.
        </p>
      </div>

      {data.changes.length === 0 ? (
        <p className="text-muted-foreground" data-testid="changes-none">
          This sequence changed nothing.
        </p>
      ) : (
        <ul className="divide-y rounded-lg border">
          {data.changes.map((change, index) => (
            <li
              key={`${change.sectionLabel}-${change.documentName}-${index}`}
              className="flex items-start justify-between gap-4 p-4"
              data-testid="submission-change"
              data-operation={change.operation}
            >
              <div className="space-y-1">
                <p className="font-medium">{change.documentName}</p>
                <p className="text-sm text-muted-foreground">
                  {change.documentTypeName} · {change.sectionLabel}
                </p>
              </div>

              <div className="flex flex-col items-end gap-1">
                <span className="text-sm font-medium">{change.operation}</span>

                {/* "v2 replaced v1" reads; a pair of ids does not. */}
                {change.replacesDocumentVersionNumber !== null && (
                  <span className="text-sm text-muted-foreground">
                    {change.documentVersionNumber !== null
                      ? `v${change.documentVersionNumber} replaced `
                      : "withdrew "}
                    v{change.replacesDocumentVersionNumber}
                  </span>
                )}

                {change.replacesDocumentVersionNumber === null &&
                  change.documentVersionNumber !== null && (
                    <span className="text-sm text-muted-foreground">
                      v{change.documentVersionNumber}
                    </span>
                  )}
              </div>
            </li>
          ))}
        </ul>
      )}

      {/* Carried forward untouched. A count, not rows: in a cumulative dossier
          most of a filing is unchanged, and listing it would bury the part
          that is not. */}
      <p className="text-sm text-muted-foreground" data-testid="changes-unchanged">
        {data.unchangedCount === 1
          ? "1 document carried forward unchanged."
          : `${data.unchangedCount} documents carried forward unchanged.`}
      </p>
    </div>
  );
}
