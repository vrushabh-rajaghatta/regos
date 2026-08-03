export interface ApplicationDetail {
  id: string;
  name: string;
  /** Null until the authority has assigned one. */
  applicationNumber: string | null;
  globalProductId: string;
  countryId: string;
  countryName: string;
  authorityId: string;
  authorityName: string;
  applicantOrganizationId: string;
  applicantOrganizationName: string;
  status: string;
  createdOn: string;
}
