/**
 * One market a product is present in — the tier between the global product and
 * the licences held over it.
 *
 * A product can be present in a market for years with no registration at all:
 * dossier preparation, labelling and launch planning all precede authorisation.
 * So an empty registration list under a market is ordinary, not incomplete.
 */
export interface MedicinalProduct {
  medicinalProductId: string;
  countryId: string;
  countryName: string;
  countryCode: string;
  /** The record's own lifecycle — not whether the product is on sale there. */
  status: string;
  statusDate: string;
}
