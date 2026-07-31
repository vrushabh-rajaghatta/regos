/** A row in "where is this product registered?" — the market is the answer. */
export interface RegistrationSummary {
  registrationId: string;
  countryId: string;
  countryName: string;
  authorityId: string;
  authorityName: string;
  holderOrganizationName: string;
  registrationNumber: string | null;
  status: string;
  approvedOn: string | null;
  expiresOn: string | null;
}
