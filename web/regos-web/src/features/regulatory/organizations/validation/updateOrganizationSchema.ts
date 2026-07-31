import { z } from "zod";

export const updateOrganizationSchema = z.object({

  legalName: z.string().trim().min(1, "Legal name is required."),

  type: z.string().trim().min(1, "Organization type is required."),

  // Optional: most companies file under one name in one script. Blank clears
  // the field rather than failing, which is what an empty input means here.
  acronym: z.string().trim().max(50, "Acronym is too long.").optional(),

  nameNativeLanguage: z
    .string()
    .trim()
    .max(200, "Native-language name is too long.")
    .optional(),
});

export type UpdateOrganizationFormValues = z.infer<
  typeof updateOrganizationSchema
>;
