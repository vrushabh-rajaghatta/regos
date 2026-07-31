/** What the product is called in one market, in one language. */
export interface TradeName {
  tradeNameId: string;
  /** An ISO 639-1 code. The screen renders the readable name. */
  language: string;
  name: string;
}

/**
 * One market a product is present in — the tier between the global product and
 * the licences held over it.
 *
 * A product can be present in a market for years with no registration at all:
 * dossier preparation, labelling and launch planning all precede authorisation.
 * So an empty registration list under a market is ordinary, not incomplete —
 * and so is an empty trade-name list, because branding is settled after entry.
 */
export interface MedicinalProduct {
  medicinalProductId: string;
  countryId: string;
  countryName: string;
  countryCode: string;
  /** The record's own lifecycle — not whether the product is on sale there. */
  status: string;
  statusDate: string;
  /** At most one per language, enforced by the server. */
  tradeNames: TradeName[];
}
