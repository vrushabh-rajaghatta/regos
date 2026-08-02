export interface SubmissionDetail {
  id: string;
  title: string;
  applicationId: string;
  applicationName: string;
  submissionTypeId: string;
  submissionTypeName: string;
  status: string;
  createdOn: string;
  /** What it was filed as. Null while a draft. */
  sequenceNumber: number | null;
  /** What it would be filed as if published now. A projection, never stored. */
  nextSequenceNumber: number;
  /** Its own lifecycle, oldest first — only steps we are the actor of. */
  history: SubmissionStatusStep[];
}

export interface SubmissionStatusStep {
  status: string;
  /** When it happened, as a regulator would date it. */
  occurredOn: string;
  /** When RegOS was told. */
  recordedOnUtc: string;
  note: string | null;
}
