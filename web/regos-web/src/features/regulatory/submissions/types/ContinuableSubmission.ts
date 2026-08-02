/** A published sequence that opened an activity a new one could continue. */
export interface ContinuableSubmission {
  id: string;
  sequenceNumber: number;
  title: string;
  /** What the activity is — "Annual Report", "Original Application". */
  submissionTypeName: string;
}
