import { z } from "zod";

/**
 * Shape and required-ness only.
 *
 * `interactant` is required here as well as on the server, and that is not
 * duplication of a business rule — it is the form refusing to submit something
 * it can see is incomplete. The rule that an interaction must *keep* at least
 * one interactant lives only in the aggregate, where it can count them.
 */
export const recordInteractionSchema = z.object({
  interactionTypeCode: z.string().min(1, "A kind of interaction is required."),

  labelText: z
    .string()
    .trim()
    .min(1, "The wording the label uses is required.")
    .max(4000, "Label text must be 4000 characters or fewer."),

  interactant: z
    .string()
    .trim()
    .min(1, "Name what this product interacts with.")
    .max(250, "An interactant must be 250 characters or fewer."),

  interactantSubstanceId: z.string().optional(),

  management: z
    .string()
    .trim()
    .max(2000, "Management advice must be 2000 characters or fewer.")
    .optional(),

  severityCode: z.string().optional(),
});

export type RecordInteractionFormValues = z.infer<
  typeof recordInteractionSchema
>;
