/**
 * Where "soon" is decided — the only place in RegOS that has an opinion on it.
 *
 * The server returns `daysUntilExpiry`, an objective fact that never goes out of
 * date, and deliberately no `isExpiringSoon`. A threshold is policy: ninety days
 * today, a hundred and eighty tomorrow, market-specific after that,
 * tenant-configurable eventually. Keeping it here means changing it never
 * touches the domain — and when it does become configurable, this is the single
 * call site to replace.
 */
const ATTENTION_WITHIN_DAYS = 90;

export function needsAttention(daysUntilExpiry: number | null): boolean {
  return daysUntilExpiry !== null && daysUntilExpiry <= ATTENTION_WITHIN_DAYS;
}

/**
 * Reads the server's number back as English. Negative days mean the
 * authorisation lapsed and nobody has recorded it yet — the strongest signal in
 * the portfolio, so it is stated plainly rather than rounded away.
 */
export function expiryPhrase(daysUntilExpiry: number): string {
  if (daysUntilExpiry < 0) {
    const ago = Math.abs(daysUntilExpiry);
    return `lapsed ${ago} ${ago === 1 ? "day" : "days"} ago`;
  }

  if (daysUntilExpiry === 0) return "expires today";

  return `in ${daysUntilExpiry} ${daysUntilExpiry === 1 ? "day" : "days"}`;
}
