/**
 * One registry's identifier for a company. The scheme code is what the reader
 * recognises — "DUNS 150483782" — so the server sends it alongside the id.
 */
export interface OrganizationIdentifier {
  id: string;
  schemeId: string;
  schemeCode: string;
  schemeName: string;
  value: string;
}
