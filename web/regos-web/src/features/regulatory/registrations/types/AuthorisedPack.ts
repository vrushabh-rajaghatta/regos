/**
 * One pack, what authorises it, and how it is supplied.
 *
 * **The five stories of EPIC-010b in one row.** The pack and its size (S001),
 * how many layers it holds (S002), how it may be supplied and how long it keeps
 * (S003), and which licences authorise it (S005). Every fact comes from a
 * different aggregate and none of them is duplicated.
 */
export interface AuthorisedPack {
  packagedProductId: string;
  description: string;
  packSizeQuantity: number | null;
  packSizeUnitDisplay: string | null;
  packCode: string | null;
  currentMarketingStatus: string;
  legalStatusOfSupplyDisplay: string | null;
  shelfLifeValue: number | null;
  shelfLifeUnitDisplay: string | null;
  shelfLifeText: string | null;
  storageConditions: string[];
  /** A count, not the tree — "is it described?", not "how?". */
  layerCount: number;
  /**
   * **Empty is ordinary, not an error.** A pack in design has no licence yet.
   * Several is also ordinary: a partial divestment leaves one pack authorised
   * under two.
   */
  authorisations: PackAuthorisation[];
}

export interface PackAuthorisation {
  packAuthorisationId: string;
  registrationId: string;
  registrationNumber: string | null;
  registrationStatus: string;
  /**
   * Routinely later than the licence itself, which is why the relationship
   * carries a date rather than being a foreign key (ADR-061 §3).
   */
  authorisedOn: string;
}
