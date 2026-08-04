export type PackageMarketingStatus =
  | "Planned"
  | "Marketed"
  | "TemporarilyUnavailable"
  | "Discontinued";

export interface PackMarketingStatusEntry {
  id: string;
  status: PackageMarketingStatus;
  /** The business date — when this became true for the pack. */
  occurredOn: string;
  /** When RegOS learned of it. A different date, and both get asked about. */
  recordedOnUtc: string;
  note: string | null;
}

/**
 * What the market sells. Screen word **Pack**; the domain type is
 * `PackagedProduct` (ADR-061).
 *
 * `packSizeQuantity` and `packSizeUnitCode` are null together or set together —
 * the aggregate refuses half a pack size, because *30* alone could be tablets,
 * millilitres or vials.
 */
export interface Pack {
  id: string;
  description: string;
  packSizeQuantity: number | null;
  packSizeUnitCode: string | null;
  packSizeUnitDisplay: string | null;
  packSizeUnitSystem: string | null;
  /** The market's own identifier — an NDC, a national code, a PZN. */
  packCode: string | null;
  currentMarketingStatus: PackageMarketingStatus;
  currentMarketingStatusOccurredOn: string;
  history: PackMarketingStatusEntry[];
}
