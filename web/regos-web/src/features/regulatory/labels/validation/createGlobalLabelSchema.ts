import { z } from "zod";

/**
 * Shape and required-ness only. The server owns the business rules, and its
 * refusals are surfaced verbatim rather than restated here.
 */
export const createGlobalLabelSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "A label name is required.")
    .max(200, "A label name must be 200 characters or fewer."),

  labelTypeCode: z.string().min(1, "A label type is required."),
});

export type CreateGlobalLabelFormValues = z.infer<
  typeof createGlobalLabelSchema
>;
