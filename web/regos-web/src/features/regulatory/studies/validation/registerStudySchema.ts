import { z } from "zod";

/**
 * Shape and required-ness only. The server owns the business rules — that an
 * identifier names one study across both kinds is checked there, and its
 * refusal is rendered verbatim rather than restated here.
 *
 * No character rule either, deliberately: the domain does not police the
 * identifier's format (ADR-056), so a form that did would refuse data the API
 * accepts.
 */
export const registerStudySchema = z.object({
  kind: z.enum(["Clinical", "NonClinical"]),

  // Bounded to match SponsorStudyIdentifierMaxLength — the server would refuse
  // a longer one, and the field is short enough that failing here is kinder.
  sponsorStudyIdentifier: z
    .string()
    .trim()
    .min(1, "The sponsor's study ID is required.")
    .max(50, "A study ID must be 50 characters or fewer."),

  title: z
    .string()
    .trim()
    .min(1, "A study title is required.")
    .max(500, "A study title must be 500 characters or fewer."),
});

export type RegisterStudyFormValues = z.infer<typeof registerStudySchema>;
