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
  /** Carton artwork is one of these, not a separate thing (EPIC-018 D2). */
  localLabelTypes: CodedConcept[];
}

/**
 * A market's own controlled labelling document. The domain calls it a
 * `LocalLabel`; the screen calls it a **local label**.
 *
 * `approvedOn` and `effectiveFrom` are two fields on purpose — *approved 12
 * May, effective 1 June* and *approved 12 May, effective immediately* both
 * occur, and a screen showing one date could not tell them apart.
 */
export interface LocalLabel {
  id: string;
  labelTypeCode: string;
  labelTypeDisplay: string;
  labelTypeSystem: string;
  language: string;
  revisionInForceNumber: number | null;
  approvedOn: string | null;
  effectiveFrom: string | null;
  draftRevisionId: string | null;
  draftRevisionNumber: number | null;
  revisionCount: number;
}

export type LocalLabelRevisionStatus = "Draft" | "InForce" | "Superseded";

/**
 * `derivedFromGlobalLabelVersionNumber` being null is legitimate, not an error:
 * a migrated portfolio does not know which core version a historical revision
 * came from (EPIC-018 D3).
 */
export interface LocalLabelRevision {
  id: string;
  revisionNumber: number;
  status: LocalLabelRevisionStatus;
  contentId: string | null;
  derivedFromGlobalLabelVersionId: string | null;
  derivedFromGlobalLabelVersionNumber: number | null;
  dataCarrierCode: string | null;
  changeSummary: string | null;
  approvedOn: string | null;
  effectiveFrom: string | null;
  effectiveTo: string | null;
}

/**
 * A core-label version a market can say it was written from.
 *
 * Flattened across the product's core labels, and including superseded ones:
 * a market catching up may be adopting a version the company has already moved
 * past, which is ordinary rather than an error.
 */
export interface CoreVersionOption {
  id: string;
  globalLabelId: string;
  labelName: string;
  versionNumber: number;
  status: string;
  effectiveFrom: string | null;
}
