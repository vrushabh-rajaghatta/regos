import { z } from "zod";

/**
 * A layer holds at least one of whatever it holds — the same rule the aggregate
 * enforces, so a refusal is visible before the round trip rather than only
 * after it.
 */
export const packageItemSchema = z.object({
  itemTypeCode: z.string().min(1, "What is this layer?"),

  materialCode: z.string().trim(),

  quantity: z
    .string()
    .trim()
    .min(1, "How many of these are there?")
    .refine((v) => Number(v) > 0, "A layer holds at least one."),

  unitOfPresentationCode: z.string().trim(),

  description: z.string().trim().max(500).optional(),
});

export type PackageItemFormValues = z.infer<typeof packageItemSchema>;
