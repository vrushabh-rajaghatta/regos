export interface RegulatoryApplicationDetail {
  id: string;
  name: string;
  applicationNumber: string | null;
  status: string;
  countryName: string;
  countryCode: string;
  authorityName: string;
  authorityCode: string;
  organizationName: string;
}
