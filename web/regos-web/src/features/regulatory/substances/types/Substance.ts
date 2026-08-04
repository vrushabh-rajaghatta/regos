import type { CodedConcept } from "@/shared/types/CodedConcept";

export type { CodedConcept };

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

export { REGOS_INTERNAL } from "@/shared/types/CodedConcept";
