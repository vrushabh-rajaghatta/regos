import { z } from "zod";

/**
 * Deliberately permissive. FDA issues six digits, Health Canada and the EMA
 * issue something else, and RegOS records what the authority assigned — so the
 * only client-side rule is that something was entered (ADR-055). The
 * FDA-specific shape is checked when an FDA package is generated, which is the
 * boundary that cares.
 */
export const recordApplicationNumberSchema = z.object({
  applicationNumber: z
    .string()
    .trim()
    .min(1, "Enter the number the authority assigned."),
});

export type RecordApplicationNumberFormValues = z.infer<
  typeof recordApplicationNumberSchema
>;
