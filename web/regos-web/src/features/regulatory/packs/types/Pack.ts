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

  /**
   * Who may hand this pack over. Per pack, not per product — a 16-tablet pack
   * of paracetamol may be general sale where a 100-tablet pack is
   * pharmacy-only (ADR-061 §1). Null until it is classified.
   */
  legalStatusOfSupplyCode: string | null;
  legalStatusOfSupplyDisplay: string | null;

  /**
   * How long it keeps. Literal — *3 years* arrives as three years, never
   * normalised to thirty-six months.
   */
  shelfLifeValue: number | null;
  shelfLifeUnitCode: string | null;
  shelfLifeUnitDisplay: string | null;

  /** What the label says, in the words it was approved in. */
  shelfLifeText: string | null;

  /**
   * Empty means nobody has stated any. A lone `NO_SPECIAL_PRECAUTIONS` means
   * somebody checked and none are needed — a different statement, kept
   * distinguishable.
   */
  storageConditions: PackStorageCondition[];

  /**
   * The long-term conditions the shelf life was demonstrated at.
   *
   * **Not `storageConditions`, which is one field up and sounds alike.** Those
   * are label instructions — *"do not store above 25 °C"*. These are study
   * conditions, and only these decide whether the period holds in a given
   * market. Empty means the stability data has not been recorded, which is not
   * a rejection.
   */
  testedAt: PackTestedAt[];

  history: PackMarketingStatusEntry[];
}

export interface PackStorageCondition {
  code: string;
  display: string;
}

/** Identical in shape to `PackStorageCondition` and named apart on purpose. */
export interface PackTestedAt {
  code: string;
  display: string;
}
