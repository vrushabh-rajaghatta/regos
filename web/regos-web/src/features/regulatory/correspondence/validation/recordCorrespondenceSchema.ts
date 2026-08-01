import { z } from "zod";

/**
 * Shape and required-ness only.
 *
 * The chronology rule — a response cannot be due before the letter it answers —
 * is deliberately **not** mirrored here (SC-103). It is a business rule, it
 * lives in the aggregate, and the server's own `detail` message is surfaced
 * verbatim when it refuses. Duplicating it would give the codebase two places
 * to change it and one of them would eventually be wrong.
 */
export const recordCorrespondenceSchema = z.object({
  authorityId: z.string().min(1, "A health authority is required."),

  correspondenceTypeId: z.string().min(1, "A type is required."),

  direction: z.enum(["Inbound", "Outbound"], {
    message: "Say whether it was received or sent.",
  }),

  subject: z
    .string()
    .min(1, "A subject is required.")
    .max(300, "A subject cannot exceed 300 characters."),

  // The date printed on the letter, never defaulted to today: a letter logged
  // now may be from 2019.
  occurredOn: z.string().min(1, "A date is required."),

  responseDueOn: z.string().optional(),

  authorityReference: z
    .string()
    .max(100, "A reference cannot exceed 100 characters.")
    .optional(),

  authorityDivisionId: z.string().optional(),

  regulatoryApplicationId: z.string().optional(),
});

export type RecordCorrespondenceFormValues = z.infer<
  typeof recordCorrespondenceSchema
>;
