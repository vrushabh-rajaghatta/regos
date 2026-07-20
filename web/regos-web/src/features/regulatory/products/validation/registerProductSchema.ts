import { z } from "zod";

export const registerProductSchema = z.object({
  // Mirrors the ProductCode value object: the API normalizes to upper case and
  // rejects anything outside this set, so the form fails fast with the same rule.
  code: z
    .string()
    .trim()
    .min(1, "Product code is required.")
    .max(50, "Product code must be 50 characters or fewer.")
    .regex(
      /^[A-Za-z0-9_-]+$/,
      "Product code may contain only letters, digits, dashes and underscores.",
    ),

  name: z.string().trim().min(1, "Product name is required."),

  type: z.enum(["Drug", "MedicalDevice", "Biologic"]),
});

export type RegisterProductFormValues = z.infer<typeof registerProductSchema>;
