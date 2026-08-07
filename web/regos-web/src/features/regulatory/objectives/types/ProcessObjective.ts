export interface ObjectiveSummary {
  id: string;
  name: string;
  productName: string;
  countryCode: string;
  countryName: string;
  status: string;
  statedOn: string;
  targetCompletionOn: string | null;
  achievedOn: string | null;
  /** Whether the market-local record exists yet. False is normal, not a gap. */
  hasMarketRecord: boolean;
  ownerUserId: string | null;
}

export interface ObjectiveHistoryEntry {
  status: string;
  /** The business date — when it became true. */
  occurredOn: string;
  /** When RegOS learned. Two clocks, always. */
  recordedOnUtc: string;
  note: string | null;
}

export interface ObjectiveDetail {
  id: string;
  name: string;
  /** Why this, and why this route — the strategy content. */
  rationale: string | null;
  globalProductId: string;
  productName: string;
  countryCode: string;
  countryName: string;
  medicinalProductId: string | null;
  regulatoryApplicationId: string | null;
  ownerUserId: string | null;
  status: string;
  statedOn: string;
  targetCompletionOn: string | null;
  achievedOn: string | null;
  history: ObjectiveHistoryEntry[];
}
