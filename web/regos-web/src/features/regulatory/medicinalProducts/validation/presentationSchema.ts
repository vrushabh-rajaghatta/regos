import { z } from "zod";

/**
 * Shape and required-ness only. The server owns the vocabulary — a dose form or
 * route RegOS does not know is refused there, with a message naming what it
 * would have accepted, and that message is rendered verbatim rather than
 * restated here.
 */
export const presentationSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "A presentation name is required.")
    .max(250, "A presentation name must be 250 characters or fewer."),

  description: z
    .string()
    .trim()
    .max(2000, "A description must be 2000 characters or fewer.")
    .optional(),

  doseFormCode: z.string().min(1, "A dose form is required."),

  // Optional because an oral solution measured in mL has no article to count.
  unitOfPresentationCode: z.string().optional(),

  // Empty is allowed: a presentation may be recorded before the route is
  // settled, and the server accepts none.
  routeCodes: z.array(z.string()),
});

export type PresentationFormValues = z.infer<typeof presentationSchema>;
