import type { StudyKind } from "../types/Study";

/**
 * The screen's words for the two kinds, and the route segment each registers
 * through.
 *
 * `NonClinical` is the domain's word; **"Non-clinical"** is the screen's. Kept
 * as a pair here rather than hyphenating at every call site.
 */
export const STUDY_KINDS: {
  value: StudyKind;
  label: string;
  path: "clinical" | "nonclinical";
  hint: string;
}[] = [
  {
    value: "NonClinical",
    label: "Non-clinical",
    path: "nonclinical",
    hint: "Toxicology, pharmacology — CTD Module 4.",
  },
  {
    value: "Clinical",
    label: "Clinical",
    path: "clinical",
    hint: "In human subjects — CTD Module 5.",
  },
];

export const studyKindLabel = (kind: StudyKind): string =>
  STUDY_KINDS.find((k) => k.value === kind)?.label ?? kind;
