/** eCTD's `submission-sub-type` — what one sequence does to its activity. */
export interface SubmissionSubTypeOption {
  id: string;
  code: string;
  name: string;
  token: string | null;
  authorityId: string;
}
