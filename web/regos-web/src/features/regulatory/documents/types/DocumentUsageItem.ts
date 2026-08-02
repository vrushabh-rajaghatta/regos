/**
 * One event in a document's filing history.
 *
 * Placements and withdrawals arrive as one stream. The write model keeps them
 * apart because an absence cannot be frozen (ADR-045); the read puts them back
 * together, and `operation` is what tells them apart.
 */
export interface DocumentUsageItem {
  submissionId: string;
  applicationId: string;
  applicationName: string;
  submissionTitle: string;
  submissionType: string;
  authority: string;
  /** What the filing was numbered. Null while a draft. */
  sequenceNumber: number | null;
  status: string;
  /** What the filing was rendered as — the domain's word, see `formatLabel`. */
  format: string;
  /** `New` | `Replace` | `Unchanged` | `Delete`. Null while a draft. */
  operation: string | null;
  /** Null exactly when the event is a withdrawal — nothing was placed. */
  versionNumber: number | null;
  /** Null for a withdrawal, for the same reason. */
  attachedOnUtc: string | null;
}
