import { z } from "zod";

/**
 * No rule here pairing an age with its unit, or checking the range runs the
 * right way.
 *
 * Both are the aggregate's, both refuse with a sentence worth reading — *"2 to
 * 12 could be months or years"* — and duplicating them here would mean two
 * checks that can disagree.
 */
export const populationSchema = z.object({
  ageLow: z.string().optional(),
  ageHigh: z.string().optional(),
  ageUnitCode: z.string().optional(),
  genderCode: z.string().min(1, "A gender is required."),
  physiologicalConditionCode: z.string().optional(),
  description: z
    .string()
    .trim()
    .max(500, "A description must be 500 characters or fewer.")
    .optional(),
});

export type PopulationFormValues = z.infer<typeof populationSchema>;

/** The sentinel a Select uses for "none", since "" cannot be an item value. */
export const NONE = "__none__";

export const chosen = (value: string | undefined): string | null =>
  value && value !== NONE ? value : null;
