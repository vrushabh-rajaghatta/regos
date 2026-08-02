/** One person named on a filing, and what they were named as (ADR-048). */
export interface SubmissionRole {
  id: string;
  contactId: string;
  contactName: string;
  contactTitle: string | null;
  organizationName: string;
  roleId: string;
  roleName: string;
}
