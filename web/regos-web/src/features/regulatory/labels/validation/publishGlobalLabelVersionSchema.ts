import { z } from "zod";

/**
 * No rule here about the date being after the version it replaces.
 *
 * That rule needs the label's current state to evaluate, the server has it, and
 * its refusal names the boundary. A client-side copy would either be wrong or
 * would need the same state fetched twice — and the one thing worse than no
 * check is two checks that disagree.
 */
export const publishGlobalLabelVersionSchema = z.object({
  effectiveFrom: z.string().min(1, "A date is required."),

  changeSummary: z
    .string()
    .trim()
    .max(2000, "A change summary must be 2000 characters or fewer.")
    .optional(),
});

export type PublishGlobalLabelVersionFormValues = z.infer<
  typeof publishGlobalLabelVersionSchema
>;
