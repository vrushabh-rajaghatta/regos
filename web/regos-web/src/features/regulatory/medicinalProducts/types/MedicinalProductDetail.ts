import type { TradeName } from "./MedicinalProduct";

/**
 * One point in a market's commercial history, carrying both of its dates.
 *
 * They are never conflated. `occurredOn` is when it became true in the market;
 * `recordedOnUtc` is when RegOS learned of it. A launch backdated to 2021 and
 * entered today shows both — and that difference is the reason the second
 * timestamp exists at all.
 */
export interface MarketStatusEntry {
  id: string;
  status: string;
  occurredOn: string;
  recordedOnUtc: string;
  note: string | null;
}

export interface MedicinalProductDetail {
  medicinalProductId: string;
  globalProductId: string;
  productName: string;
  productCode: string;
  countryId: string;
  countryName: string;
  countryCode: string;
  /** Whether this record participates in normal work. */
  status: string;
  statusDate: string;
  /** Whether the product is on sale — a different question from `status`. */
  marketStatus: string;
  /**
   * As the tenant supplied it, and not verified — RegOS holds no WHO ATC index.
   * A plain string because that is the whole of the claim.
   */
  atcCode: string | null;
  /** Derived from the history, never stored. */
  launchedOn: string | null;
  tradeNames: TradeName[];
  marketStatusHistory: MarketStatusEntry[];
}
