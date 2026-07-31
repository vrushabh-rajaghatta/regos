/**
 * A row in "what do we hold in this market?" — the product is the answer.
 *
 * Deliberately not the same shape as `RegistrationSummary`: the two views are
 * mirror images, and a single type carrying both axes would leave every
 * consumer ignoring half its fields.
 */
export interface MarketRegistrationSummary {
  registrationId: string;
  productId: string;
  productCode: string;
  productName: string;
  authorityId: string;
  authorityName: string;
  holderOrganizationName: string;
  registrationNumber: string | null;
  status: string;
  approvedOn: string | null;
  expiresOn: string | null;
}
