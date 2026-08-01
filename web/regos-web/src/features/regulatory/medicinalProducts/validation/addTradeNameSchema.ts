import { z } from "zod";

export const addTradeNameSchema = z.object({
  // Mirrors the LanguageCode value object: two ASCII letters, ISO 639-1. The
  // server normalizes and is the authority; this only fails fast on the shape.
  language: z
    .string()
    .trim()
    .regex(/^[A-Za-z]{2}$/, "A language is required."),

  name: z
    .string()
    .trim()
    .min(1, "A trade name is required.")
    .max(200, "A trade name must be 200 characters or fewer."),
});

export type AddTradeNameFormValues = z.infer<typeof addTradeNameSchema>;
