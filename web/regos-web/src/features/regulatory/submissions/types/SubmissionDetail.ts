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
}
