export interface SubmissionChange {
  /** "New", "Replace" or "Delete". Never "Unchanged" — those are counted. */
  operation: string;
  documentName: string;
  documentTypeName: string;
  sectionLabel: string;
  documentVersionNumber: number | null;
  /** The version this superseded, for a Replace or a Delete. */
  replacesDocumentVersionNumber: number | null;
}

export interface SubmissionChanges {
  /** Null while the submission is a draft — nothing has been filed. */
  sequenceNumber: number | null;
  previousSequenceNumber: number | null;
  changes: SubmissionChange[];
  /** Placements carried forward untouched. */
  unchangedCount: number;
}
