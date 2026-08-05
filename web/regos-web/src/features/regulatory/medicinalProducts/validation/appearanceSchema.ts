import { z } from "zod";

/**
 * The same limits the value object enforces, so a refusal is visible before the
 * round trip rather than only after it.
 *
 * There is deliberately no "at least one field" rule: an appearance nobody has
 * described is an ordinary state, and clearing every field is how a description
 * entered in error is withdrawn.
 */
export const appearanceSchema = z.object({
  colourCodes: z.array(z.string()),

  shapeCode: z.string().trim(),

  imprint: z
    .string()
    .trim()
    .max(100, "Record what is stamped on it, not how it looks.")
    .optional(),

  description: z.string().trim().max(1000).optional(),
});

export type AppearanceFormValues = z.infer<typeof appearanceSchema>;
