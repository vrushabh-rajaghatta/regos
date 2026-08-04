import type { CodedConcept } from "@/shared/types/CodedConcept";

export type { CodedConcept };
export { REGOS_INTERNAL } from "@/shared/types/CodedConcept";

/** Approved, expanded, restricted, withdrawn — successive decisions. */
export type IndicationStatus =
  | "Approved"
  | "Expanded"
  | "Restricted"
  | "Withdrawn";

/**
 * Who a statement applies to.
 *
 * `ageLow` null means from birth; `ageHigh` null means and above. Neither is
 * missing data, and the screen says so rather than showing a blank.
 */
export interface Population {
  id: string;
  ageLow: number | null;
  ageHigh: number | null;
  ageUnitCode: string | null;
  ageUnitDisplay: string | null;
  genderCode: string;
  genderDisplay: string;
  physiologicalConditionCode: string | null;
  physiologicalConditionDisplay: string | null;
  description: string | null;
}

export interface OtherTherapy {
  id: string;
  relationshipCode: string;
  relationshipDisplay: string;
  therapy: string;
}

/** `recordedOnUtc` is when RegOS learned of it; `occurredOn` is when it took effect. */
export interface IndicationDecision {
  id: string;
  status: IndicationStatus;
  occurredOn: string;
  recordedOnUtc: string;
  note: string | null;
}

/**
 * What this product is approved to treat in one market.
 *
 * `conditionCode` is the join key — the same authorisation in Japan and France
 * shares it, and the label texts do not.
 */
export interface Indication {
  id: string;
  conditionCode: string;
  conditionDisplay: string;
  conditionSystem: string;
  labelText: string;
  currentStatus: IndicationStatus;
  currentStatusOccurredOn: string;
  populations: Population[];
  otherTherapies: OtherTherapy[];
  history: IndicationDecision[];
}

export interface ClinicalVocabulary {
  conditions: CodedConcept[];
  physiologicalConditions: CodedConcept[];
  genders: CodedConcept[];
  ageUnits: CodedConcept[];
  therapyRelationships: CodedConcept[];
}

export interface PopulationBody {
  ageLow: number | null;
  ageHigh: number | null;
  ageUnitCode: string | null;
  genderCode: string;
  physiologicalConditionCode: string | null;
  description: string | null;
}
