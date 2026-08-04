import { z } from "zod";

/**
 * Shape and required-ness only. The server owns the business rules and its
 * refusals are surfaced verbatim.
 */
export const createLocalLabelSchema = z.object({
  labelTypeCode: z.string().min(1, "A label type is required."),

  language: z
    .string()
    .trim()
    .length(2, "A language is a two-letter code, such as en or ja."),
});

export type CreateLocalLabelFormValues = z.infer<
  typeof createLocalLabelSchema
>;
