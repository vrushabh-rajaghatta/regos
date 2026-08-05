import { z } from "zod";

/**
 * What the unit pickers hold when nothing is chosen.
 *
 * A sentinel rather than an empty string because Radix's Select refuses `""` as
 * an item value. **It lives here, not in the form**, because the rules below
 * have to recognise it — a schema that read `"__none__"` as a chosen unit would
 * refuse an excipient with no strength, which is the ordinary case.
 */
export const NO_UNIT = "__none__";

const chosen = (value?: string) => !!value && value !== NO_UNIT;

/**
 * Shape and required-ness only, with one exception worth stating.
 *
 * **"An active must declare a strength" is checked here as well as on the
 * server**, and that is a deliberate duplication rather than a slip. The rule
 * decides which fields the form itself requires, so a form that did not know it
 * could not mark them — and the alternative is submitting a form to be told a
 * field beside the cursor was needed. The server still owns it; this copy only
 * decides what to ask for.
 *
 * Everything else is the server's: whether a unit exists, whether the substance
 * is already in the composition, and whether removing this would leave a
 * formulation with no active.
 */
export const ingredientSchema = z
  .object({
    substanceId: z.string().min(1, "A substance is required."),

    role: z.enum(["Active", "Excipient"]),

    // Strings, not numbers: an empty numeric input is NaN, and NaN is not the
    // same fact as "not declared".
    numeratorValue: z.string().trim().optional(),
    numeratorUnitCode: z.string().optional(),
    denominatorValue: z.string().trim().optional(),
    denominatorUnitCode: z.string().optional(),

    // No rule of its own, and none is missing. Provenance is optional for
    // every role — an excipient has a supplier like anything else — and the
    // empty string means "nobody has said" rather than "unsourced".
    manufacturingSourceSiteId: z.string().optional(),
  })
  .superRefine((values, context) => {
    const hasNumerator = (values.numeratorValue ?? "") !== "";

    if (values.role === "Active" && !hasNumerator) {
      context.addIssue({
        code: "custom",
        path: ["numeratorValue"],
        message:
          "An active ingredient must declare a strength — it is what the "
          + "product is dosed by.",
      });
    }

    if (hasNumerator && !chosen(values.numeratorUnitCode)) {
      context.addIssue({
        code: "custom",
        path: ["numeratorUnitCode"],
        message: "A strength needs a unit.",
      });
    }

    if (hasNumerator && Number.isNaN(Number(values.numeratorValue))) {
      context.addIssue({
        code: "custom",
        path: ["numeratorValue"],
        message: "A strength is a number.",
      });
    }

    // Half a fraction is not a smaller fraction, it is a broken one — the same
    // pairing the value object enforces.
    const hasDenominatorValue = (values.denominatorValue ?? "") !== "";
    const hasDenominatorUnit = chosen(values.denominatorUnitCode);

    if (hasDenominatorValue !== hasDenominatorUnit) {
      context.addIssue({
        code: "custom",
        path: ["denominatorValue"],
        message:
          "A strength per a quantity needs both the quantity and its unit — "
          + "10 mg per 1 mL.",
      });
    }
  });

export type IngredientFormValues = z.infer<typeof ingredientSchema>;
