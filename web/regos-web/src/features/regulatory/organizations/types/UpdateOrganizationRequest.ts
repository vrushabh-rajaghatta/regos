export interface UpdateOrganizationRequest {
  legalName: string;
  type: string;
  acronym: string | null;
  nameNativeLanguage: string | null;
}
