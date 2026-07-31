const LABELS: Record<string, string> = {
  UnderReview: "Under Review",
};

/**
 * How a status is written for a person. Presentation only — the set of statuses
 * and what may follow which are the server's business.
 */
export function statusLabel(status: string): string {
  return LABELS[status] ?? status;
}
