import { z } from "zod";

/**
 * Shape and required-ness only. The vocabulary and the business rules are the
 * server's, and its refusals render verbatim.
 */
export const recordStatementSchema = z.object({
  conditionCode: z.string().min(1, "A condition is required."),

  labelText: z
    .string()
    .trim()
    .min(1, "The wording the label uses is required.")
    .max(4000, "Label text must be 4000 characters or fewer."),

  /** Undesirable effects only. Optional there too — a label may not state one. */
  frequencyCode: z.string().optional(),
});

export type RecordStatementFormValues = z.infer<typeof recordStatementSchema>;
