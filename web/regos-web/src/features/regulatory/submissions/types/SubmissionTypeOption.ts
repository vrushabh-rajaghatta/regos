/** eCTD's `submission-type` — what a regulatory activity is. */
export interface SubmissionTypeOption {
  id: string;
  code: string;
  name: string;
  /**
   * The eCTD wire value, or null when it is not in evidence. A choice with no
   * token can be recorded but cannot be rendered into a package, and the screen
   * says so rather than letting the failure arrive at package time.
   */
  token: string | null;
  authorityId: string;
}
