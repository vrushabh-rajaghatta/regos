/**
 * A registration whose validity period is still running.
 *
 * Carries both axes, unlike the portfolio summaries: this list spans the whole
 * book, so neither the product nor the market is implied by where you stand.
 */
export interface ExpiringRegistration {
  registrationId: string;
  productId: string;
  productName: string;
  countryId: string;
  countryName: string;
  registrationNumber: string | null;
  status: string;
  expiresOn: string;
  /** Never null here — negative once the date has passed. */
  daysUntilExpiry: number;
  isExpired: boolean;
}
