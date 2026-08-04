/**
 * A term drawn from a controlled vocabulary.
 *
 * `system` travels to the client on purpose. Every term RegOS ships today is
 * `regos-internal`, and a screen showing "Tablet" without saying whose word it
 * is implies terminology the platform does not hold (ADR-058 §6).
 */
export interface CodedValue {
  system: string;
  code: string;
  display: string;
}

/**
 * What a product physically is in one market. The domain calls it a
 * `PharmaceuticalProductDetail`; the screen calls it a **Presentation**.
 *
 * A market may have several — 10 mg, 20 mg and 40 mg tablets are one commercial
 * presence with three presentations.
 */
export interface Presentation {
  presentationId: string;
  medicinalProductId: string;
  name: string;
  description: string | null;
  doseForm: CodedValue;
  /** Null when there is no article to count — an oral solution measured in mL. */
  unitOfPresentation: CodedValue | null;
  routesOfAdministration: CodedValue[];
}

/**
 * `unitsOfPresentation` counts articles — a vial, a tablet. It is **not** a
 * list of strength units; mg, mL and IU measure quantity and arrive with
 * ingredients.
 */
export interface PharmaceuticalVocabulary {
  doseForms: CodedValue[];
  routesOfAdministration: CodedValue[];
  unitsOfPresentation: CodedValue[];
}

/**
 * What add and restate both send — the same five facts, because a presentation
 * that could be restated into a state it could not be created in would be a
 * gap, not a feature.
 */
export interface PresentationBody {
  name: string;
  description: string | null;
  doseFormCode: string;
  unitOfPresentationCode: string | null;
  routeCodes: string[];
}

export const REGOS_INTERNAL = "regos-internal";
