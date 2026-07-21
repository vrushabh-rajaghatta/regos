export interface CreateTenantRequest {
  name: string;
  /** What the customer is, regulatorily — the mirror organization's type. */
  organizationType:
    | "Manufacturer"
    | "Sponsor"
    | "MarketingAuthorizationHolder"
    | "ContractResearchOrganization";
  adminEmail: string;
  adminFirstName: string;
  adminLastName: string;
}
