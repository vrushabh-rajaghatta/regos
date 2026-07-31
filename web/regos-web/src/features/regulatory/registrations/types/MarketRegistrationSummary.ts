/**
 * A row in "what do we hold in this market?" — the product is the answer.
 *
 * Deliberately not the same shape as `RegistrationSummary`: the two views are
 * mirror images, and a single type carrying both axes would leave every
 * consumer ignoring half its fields.
 */
export interface MarketRegistrationSummary {
  registrationId: string;
  globalProductId: string;
  productCode: string;
  productName: string;
  authorityId: string;
  authorityName: string;
  holderOrganizationName: string;
  registrationNumber: string | null;
  status: string;
  approvedOn: string | null;
  expiresOn: string | null;
  /** Whether the registration is still on the validity timeline at all. */
  hasRunningValidity: boolean;
  /**
   * Days until it lapses. Null when there is no expiry date or the lifecycle
   * has ended; negative once the date has passed. Derived by the server on
   * every read — never stored, and never a judgement about what is urgent.
   */
  daysUntilExpiry: number | null;
  isExpired: boolean;
}
