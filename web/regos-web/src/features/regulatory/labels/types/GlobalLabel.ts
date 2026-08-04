import type { CodedConcept } from "@/shared/types/CodedConcept";

export type { CodedConcept };
export { REGOS_INTERNAL } from "@/shared/types/CodedConcept";

/**
 * A label a company holds centrally, above any market. The domain calls it a
 * `GlobalLabel`; the screen calls it a **global label** — and may one day call
 * it a CCDS, which is a wording change and not a modelling one (ADR-059 §8).
 *
 * `versionInForceNumber` is null while the first draft is still being written.
 * That is an ordinary state, not an error, and the screen says so.
 */
export interface GlobalLabel {
  id: string;
  name: string;
  labelTypeCode: string;
  labelTypeDisplay: string;
  labelTypeSystem: string;
  versionInForceNumber: number | null;
  effectiveFrom: string | null;
  draftVersionId: string | null;
  draftVersionNumber: number | null;
  versionCount: number;
}

/** Draft → InForce → Superseded. At most one of each of the first two. */
export type GlobalLabelVersionStatus = "Draft" | "InForce" | "Superseded";

/**
 * `publishedOnUtc` and `effectiveFrom` are deliberately two fields: a version
 * approved in March to apply from June has two dates, and somebody asks about
 * each.
 */
export interface GlobalLabelVersion {
  id: string;
  versionNumber: number;
  status: GlobalLabelVersionStatus;
  contentId: string | null;
  changeSummary: string | null;
  effectiveFrom: string | null;
  effectiveTo: string | null;
  publishedOnUtc: string | null;
}

export interface LabelVocabulary {
  globalLabelTypes: CodedConcept[];
}
