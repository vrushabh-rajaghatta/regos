import { z } from "zod";

/**
 * Shape and required-ness only. The server owns the vocabulary and the business
 * rules, and its refusals are surfaced verbatim — including the one that says
 * a condition is not in the RegOS clinical vocabulary, which tells a user which
 * list was consulted rather than that the condition does not exist.
 */
export const recordIndicationSchema = z.object({
  conditionCode: z.string().min(1, "A condition is required."),

  labelText: z
    .string()
    .trim()
    .min(1, "The wording the label uses is required.")
    .max(4000, "Label text must be 4000 characters or fewer."),

  approvedOn: z.string().min(1, "An approval date is required."),
});

export type RecordIndicationFormValues = z.infer<typeof recordIndicationSchema>;
