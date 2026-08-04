import { z } from "zod";

/**
 * No rule here about effect following approval, or about the date being after
 * the revision in force.
 *
 * Both need state the server already holds, and its refusals name the boundary.
 * A client-side copy would either be wrong or need the same state fetched
 * twice — and two checks that disagree are worse than one.
 */
export const publishLocalLabelRevisionSchema = z.object({
  approvedOn: z.string().min(1, "An approval date is required."),
  effectiveFrom: z.string().min(1, "An effective date is required."),
});

export type PublishLocalLabelRevisionFormValues = z.infer<
  typeof publishLocalLabelRevisionSchema
>;
