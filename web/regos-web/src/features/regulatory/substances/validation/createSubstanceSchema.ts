import { z } from "zod";

/**
 * Shape and required-ness only. The server owns the business rules — that a
 * name is not already in the catalogue is checked there, and its refusal is
 * rendered verbatim rather than restated here.
 *
 * No format rule on CAS, UNII or the molecular formula either. RegOS does not
 * hold GSRS (ADR-058 §6), so a client-side pattern would be RegOS asserting a
 * shape it has no source for — and would refuse data the API accepts.
 */
export const createSubstanceSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "A substance name is required.")
    .max(250, "A substance name must be 250 characters or fewer."),

  inn: z
    .string()
    .trim()
    .max(250, "An INN must be 250 characters or fewer.")
    .optional(),

  substanceClassCode: z.string().min(1, "A class is required."),

  substanceTypeCode: z.string().min(1, "A type is required."),

  casNumber: z
    .string()
    .trim()
    .max(50, "A CAS number must be 50 characters or fewer.")
    .optional(),

  uniiCode: z
    .string()
    .trim()
    .max(50, "A UNII code must be 50 characters or fewer.")
    .optional(),

  molecularFormula: z
    .string()
    .trim()
    .max(100, "A molecular formula must be 100 characters or fewer.")
    .optional(),

  description: z
    .string()
    .trim()
    .max(2000, "A description must be 2000 characters or fewer.")
    .optional(),
});

export type CreateSubstanceFormValues = z.infer<typeof createSubstanceSchema>;
