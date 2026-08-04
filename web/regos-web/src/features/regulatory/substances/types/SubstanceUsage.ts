import type { CodedValue } from "../../medicinalProducts/types/Presentation";
import type { StrengthValue } from "../../medicinalProducts/types/Presentation";

/**
 * One place a substance is used: a product, in a market, in a presentation, in
 * a role, at a strength.
 *
 * `marketStatus` is here because an impact assessment cares far more about a
 * launched product than a planned one — without it the reader would have to
 * open every market to tell them apart.
 */
export interface SubstanceUsage {
  globalProductId: string;
  productName: string;
  productCode: string;
  medicinalProductId: string;
  countryName: string;
  countryCode: string;
  marketStatus: string;
  presentationId: string;
  presentationName: string;
  doseForm: CodedValue;
  role: string;
  strength: StrengthValue | null;
}
