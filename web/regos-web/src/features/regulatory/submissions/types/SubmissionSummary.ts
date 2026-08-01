export interface SubmissionSummary {
  id: string;
  title: string;
  status: string;
  submissionTypeName: string;
  createdOn: string;
  /** What it was filed as. Null while a draft — a draft has no number. */
  sequenceNumber: number | null;
}
