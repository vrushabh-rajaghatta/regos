/**
 * One site, what it does for this market, and over what period.
 *
 * **The single place that says where work happens.** There is deliberately no
 * manufacturer field on a pack or a package layer, where RIM puts one — the
 * operation's own type carries that distinction (ADR-063 §3).
 */
export interface ManufacturingOperation {
  manufacturingOperationId: string;
  organizationSiteId: string;
  /** Read from the site, never copied — a copied name goes stale on rename. */
  siteName: string;
  siteCountryName: string;
  siteTypeName: string;
  /** What registries know the site as — an FEI, a DUNS. What a filing quotes. */
  siteIdentifiers: string[];
  operationCode: string;
  operationDisplay: string;
  effectiveFrom: string;
  /** Null while the site still performs it. A closed period is history. */
  ceasedOn: string | null;
  isCurrent: boolean;
}

/**
 * What a site may do for a product.
 *
 * **Its own payload rather than a fourth list on the supply vocabulary** —
 * that one answers *how may this pack be handed over, and how must it be
 * kept?*, this answers *who does the work?*, and no form states both.
 */
export interface ManufacturingVocabulary {
  operations: { system: string; code: string; display: string }[];
}
