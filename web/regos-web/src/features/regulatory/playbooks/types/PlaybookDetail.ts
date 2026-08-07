export interface PlaybookVersionSummary {
  id: string;
  versionNumber: number;
  status: string;
  effectiveFrom: string | null;
  /** Derived from the next version's start — never stored. */
  effectiveTo: string | null;
  publishedOnUtc: string | null;
  stepCount: number;
}

/**
 * An authored step. It has no dates: a definition describes work, and dates
 * exist only once a plan is instantiated from it.
 */
export interface PlaybookStep {
  id: string;
  code: string;
  name: string;
  description: string | null;
  parentStepId: string | null;
  order: number;
  /** Days after the last predecessor finishes, or after the plan's anchor. */
  offsetDays: number;
  durationDays: number;
  /** What this step waits for, by step code. */
  predecessors: string[];
}

export interface PlaybookDetail {
  id: string;
  code: string;
  name: string;
  description: string | null;
  isShared: boolean;
  countryCode: string;
  countryName: string;
  authorityCode: string;
  authorityName: string;
  applicationTypeCode: string;
  applicationTypeName: string;
  status: string;
  createdOnUtc: string;
  versions: PlaybookVersionSummary[];
  selectedVersionNumber: number | null;
  steps: PlaybookStep[];
}
