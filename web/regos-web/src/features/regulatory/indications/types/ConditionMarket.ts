import type { IndicationStatus } from "./Indication";

/**
 * One market's standing on one condition — a row of the answer to *"where is
 * this product approved for this?"*.
 *
 * Every market that has an indication for the condition appears, whatever its
 * standing. `isInForce` is what makes a row an approval; `status` is what was
 * decided. A withdrawal is a regulatory fact worth seeing, not a row to hide.
 */
export interface ConditionMarket {
  medicinalProductId: string;
  countryId: string;
  countryName: string;
  countryCode: string;
  indicationId: string;
  /** What *this* market's label says, beside the code every market shares. */
  labelText: string;
  status: IndicationStatus;
  since: string;
  isInForce: boolean;
}
