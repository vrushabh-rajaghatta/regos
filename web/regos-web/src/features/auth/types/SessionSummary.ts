export interface SessionSummary {
  id: string;
  /** Raw, exactly as the browser sent it. RegOS deliberately does not parse it. */
  userAgent: string | null;
  createdFromIp: string | null;
  createdOn: string;
  lastUsedOn: string;
  isCurrent: boolean;
}
