/** A row in "where is this product registered?" — the market is the answer. */
export interface RegistrationSummary {
  registrationId: string;
  /** The market this licence was granted over — the tier, not the country. */
  medicinalProductId: string;
  countryId: string;
  countryName: string;
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
