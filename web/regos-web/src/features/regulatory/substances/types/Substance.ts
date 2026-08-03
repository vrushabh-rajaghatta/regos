/**
 * A term drawn from a controlled vocabulary.
 *
 * `system` travels to the client on purpose. During MVP every value RegOS
 * ships is `regos-internal`, and a screen that showed "Chemical" without
 * saying whose word it is would imply an authority the platform does not have
 * (ADR-058 §6).
 */
export interface CodedConcept {
  system: string;
  code: string;
  display: string;
}

/**
 * A substance as the directory lists it.
 *
 * `isShared` says which half of the catalogue the row came from: the platform's
 * shared list, which nobody here can change, or the organisation's own.
 */
export interface Substance {
  id: string;
  name: string;
  inn: string | null;
  substanceClass: CodedConcept;
  substanceType: CodedConcept;
  casNumber: string | null;
  uniiCode: string | null;
  molecularFormula: string | null;
  description: string | null;
  isShared: boolean;
}

export interface SubstanceVocabulary {
  classes: CodedConcept[];
  types: CodedConcept[];
}

/** Which half of the directory to show. Mirrors the API's `origin`. */
export type SubstanceOrigin = "Any" | "Shared" | "Proprietary";

export const REGOS_INTERNAL = "regos-internal";
