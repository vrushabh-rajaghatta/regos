import { z } from "zod";

/**
 * Shape and required-ness only. The server owns the business rules — that the
 * product and country exist, and that a market record confirms rather than
 * redefines them — and its refusal is rendered verbatim rather than restated.
 *
 * No status field, deliberately: a new objective is always Proposed, and a form
 * offering the choice would let someone skip stating an intention before
 * committing to it.
 */
export const stateObjectiveSchema = z.object({
  globalProductId: z.string().min(1, "Choose the product."),

  countryId: z.string().min(1, "Choose the market."),

  name: z
    .string()
    .trim()
    .min(1, "Say what you are trying to achieve.")
    .max(300, "That name is 300 characters at most."),

  rationale: z
    .string()
    .trim()
    .max(4000, "That rationale is 4000 characters at most.")
    .optional(),

  targetCompletionOn: z.string().optional(),
});

export type StateObjectiveValues = z.infer<typeof stateObjectiveSchema>;
