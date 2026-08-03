export interface SubmissionSummary {
  id: string;
  title: string;
  status: string;
  applicationTypeName: string;
  /** eCTD, NeeS or paper — the domain's word (ADR-047). */
  format: string;
  createdOn: string;
  /** What it was filed as. Null while a draft — a draft has no number. */
  sequenceNumber: number | null;
}
