export interface PlaybookSummary {
  id: string;
  code: string;
  name: string;
  description: string | null;
  /** The platform's playbook rather than this tenant's own. Read-only here. */
  isShared: boolean;
  countryCode: string;
  countryName: string;
  authorityName: string;
  applicationTypeName: string;
  status: string;
  /** Null when nothing has been published — a playbook being written. */
  currentVersionNumber: number | null;
  versionCount: number;
  hasOpenDraft: boolean;
}
