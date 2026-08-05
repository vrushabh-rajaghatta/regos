/**
 * One pack, what authorises it, and how it is supplied.
 *
 * **The five stories of EPIC-010b in one row.** The pack and its size (S001),
 * how many layers it holds (S002), how it may be supplied and how long it keeps
 * (S003), and which licences authorise it (S005). Every fact comes from a
 * different aggregate and none of them is duplicated.
 */
/**
 * What this market may sell, and what it accepts stability data from.
 *
 * **An envelope rather than a column on every row.** What a market accepts is a
 * fact about the market; repeating it per pack would invite a reader to think
 * it varied between them.
 */
export interface MarketAuthorisedPacksResponse {
  /**
   * The long-term stability conditions this market accepts — Germany
   * *25 °C/60% RH* or *30 °C/65% RH*, India *30 °C/70% RH*.
   *
   * **Empty means RegOS holds none for this market**, not that the market
   * accepts none — and every pack's `stabilitySupported` is then null.
   */
  acceptsStabilityDataFrom: string[];
  packs: AuthorisedPack[];
}

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
  /** How the pack must be kept. Label instructions, and **not** `testedAt`. */
  storageConditions: string[];
  /**
   * The long-term conditions the shelf life was demonstrated at. Empty means
   * the stability data has not been recorded, which is not a rejection.
   */
  testedAt: string[];
  /**
   * Whether this market accepts the pack's stability data — derived on every
   * read and never stored.
   *
   * **Three-valued, because silence is not a refusal.** Null means the question
   * cannot be answered: the pack states no testing condition, or RegOS holds
   * none for this market.
   *
   * **Reported, never enforced.** False does not stop a pack being recorded,
   * saved or authorised.
   */
  stabilitySupported: boolean | null;
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
