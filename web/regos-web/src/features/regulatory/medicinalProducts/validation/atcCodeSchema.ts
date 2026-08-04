import { z } from "zod";

/**
 * No rule at all, deliberately — not even a length.
 *
 * The shape lives in the `AtcCode` value object, whose refusal shows the form a
 * code takes *and* says plainly that RegOS checks the shape and does not hold
 * the WHO ATC index. A length bound here would look harmless and would split
 * one rule across two places: a seven-character malformed code would get the
 * server's explanation while an eight-character one got "too long", which tells
 * the user nothing about what an ATC code is.
 *
 * Blank is valid: it clears the classification.
 */
export const atcCodeSchema = z.object({
  atcCode: z.string().trim().optional(),
});

export type AtcCodeFormValues = z.infer<typeof atcCodeSchema>;
