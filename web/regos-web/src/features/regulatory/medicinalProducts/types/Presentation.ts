import type { CodedConcept } from "@/shared/types/CodedConcept";

export type { CodedConcept };

// `CodedValue` was this feature's own name for the shape the substance
// directory already called `CodedConcept`. The label vocabulary was the third
// to need it, which is when ADR-018 says to stop copying — the type now lives
// in shared/types and the alias is kept so callers read unchanged.
export type CodedValue = CodedConcept;

/**
 * What a product physically is in one market. The domain calls it a
 * `PharmaceuticalProductDetail`; the screen calls it a **Presentation**.
 *
 * A market may have several — 10 mg, 20 mg and 40 mg tablets are one commercial
 * presence with three presentations.
 */
/**
 * How much of a substance a presentation contains.
 *
 * `denominatorValue` is null for a point strength — *500 mg*. It is set for a
 * concentration — *10 mg per 1 mL* — where the volume is part of the strength
 * rather than part of the packaging.
 *
 * Both units are measurements, never articles: the presentation already says
 * what the product comes in, so a strength never repeats it.
 */
export interface StrengthValue {
  numeratorValue: number;
  numeratorUnit: CodedValue;
  denominatorValue: number | null;
  denominatorUnit: CodedValue | null;
}

export type IngredientRole = "Active" | "Excipient";

/**
 * The role a substance plays in one presentation, at one strength.
 *
 * `substanceName` is joined by the server, never stored on the row — renaming
 * a substance renames it everywhere it appears at once.
 */
export interface Ingredient {
  ingredientId: string;
  substanceId: string;
  substanceName: string;
  substanceInn: string | null;
  role: IngredientRole;
  /** Null when nothing was declared — routine for an excipient. */
  strength: StrengthValue | null;
}

export interface Presentation {
  presentationId: string;
  medicinalProductId: string;
  name: string;
  description: string | null;
  doseForm: CodedValue;
  /** Null when there is no article to count — an oral solution measured in mL. */
  unitOfPresentation: CodedValue | null;
  routesOfAdministration: CodedValue[];
  ingredients: Ingredient[];
  /**
   * Whether the composition says what the product works by. A completeness
   * fact, not a validity one — the write path accepts an unfinished
   * composition, and the screen says it is unfinished.
   */
  hasAnActiveIngredient: boolean;
  /**
   * What it looks like. **Never null** — a presentation nobody has described
   * carries the empty statement, and `isStated` says which.
   */
  appearance: Appearance;
}

/**
 * What the medicine looks like. Screen word **Appearance**; the domain type is
 * `PhysicalCharacteristics`.
 *
 * On the presentation and not the pack, which is ADR-061 §1's discriminator
 * pointing the other way for once: a tablet looks identical in a carton of 30
 * and a carton of 100.
 */
export interface Appearance {
  /** Several is ordinary — a white body with a blue cap is two colours. */
  colours: CodedValue[];
  shape: CodedValue | null;
  /** What is stamped on it, and how a loose tablet gets identified. */
  imprint: string | null;
  description: string | null;
  isStated: boolean;
}

/** What the appearance form sends. */
export interface AppearanceBody {
  colourCodes: string[];
  shapeCode: string | null;
  imprint: string | null;
  description: string | null;
}

/**
 * `unitsOfPresentation` counts articles — a vial, a tablet. It is **not** a
 * list of strength units; mg, mL and IU measure quantity and arrive with
 * ingredients.
 */
export interface PharmaceuticalVocabulary {
  doseForms: CodedValue[];
  colours: CodedValue[];
  shapes: CodedValue[];
  routesOfAdministration: CodedValue[];
  unitsOfPresentation: CodedValue[];
  /** What a physical article can be — a vial, a kit. Overlaps
   * `unitsOfPresentation` almost entirely, and is still its own list: one says
   * what a strength is counted in, the other what the patient is handed, and
   * merging them would put "kit" in a strength picker. */
  componentTypes: CodedValue[];
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

/** What add and restate both send for an ingredient. */
export interface IngredientBody {
  role: IngredientRole;
  numeratorValue: number | null;
  numeratorUnitCode: string | null;
  denominatorValue: number | null;
  denominatorUnitCode: string | null;
}

export { REGOS_INTERNAL } from "@/shared/types/CodedConcept";
