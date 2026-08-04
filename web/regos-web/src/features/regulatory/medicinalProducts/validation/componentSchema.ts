import { z } from "zod";

/**
 * Shape and required-ness only. The vocabulary, the depth limit and the cycle
 * rule are all the server's — it is the only party that sees the whole tree,
 * and its refusals are rendered verbatim.
 */
export const componentSchema = z.object({
  componentTypeCode: z.string().min(1, "A component type is required."),

  name: z
    .string()
    .trim()
    .min(1, "A component name is required.")
    .max(250, "A component name must be 250 characters or fewer."),

  description: z
    .string()
    .trim()
    .max(2000, "A description must be 2000 characters or fewer.")
    .optional(),

  // A string, not a number: an empty numeric input is NaN, and NaN is not the
  // same fact as "not stated".
  quantity: z
    .string()
    .trim()
    .min(1, "A quantity is required.")
    .refine((value) => Number(value) > 0, "A quantity must be more than zero."),

  unitOfPresentationCode: z.string().optional(),
  doseFormCode: z.string().optional(),
});

export type ComponentFormValues = z.infer<typeof componentSchema>;
