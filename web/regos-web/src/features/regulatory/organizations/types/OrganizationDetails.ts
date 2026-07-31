import type { OrganizationIdentifier } from "./OrganizationIdentifier";

export interface OrganizationDetails {
  id: string;
  legalName: string;
  type: string;
  status: string;
  statusDate: string;
  acronym: string | null;
  nameNativeLanguage: string | null;
  identifiers: OrganizationIdentifier[];
}
