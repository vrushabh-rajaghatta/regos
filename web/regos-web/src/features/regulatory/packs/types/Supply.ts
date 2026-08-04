/**
 * The words that say how a pack may be handed over, and how it must be kept.
 *
 * **`shelfLifePeriods` is not the measurement vocabulary.** A duration is not a
 * quantity, and offering months beside milligrams is how *"500 months"* becomes
 * a legal strength.
 */
export interface SupplyVocabulary {
  legalStatuses: { system: string; code: string; display: string }[];
  storageConditions: { system: string; code: string; display: string }[];
  shelfLifePeriods: { system: string; code: string; display: string }[];
}

/**
 * The one storage condition that excludes every other — kept as a constant
 * because the form has to disable the rest when it is chosen, the same rule the
 * value object enforces server-side.
 */
export const NO_SPECIAL_PRECAUTIONS = "NO_SPECIAL_PRECAUTIONS";
