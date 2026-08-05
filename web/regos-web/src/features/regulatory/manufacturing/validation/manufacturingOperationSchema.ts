import { z } from "zod";

/**
 * The same rules the aggregate enforces, so a refusal is visible before the
 * round trip rather than only after it.
 *
 * **Every rule here has a server-side twin**, deliberately. The schema exists to
 * answer sooner, never to be the only answer — a browser is not where a
 * regulatory invariant lives.
 */
export const manufacturingOperationSchema = z.object({
  organizationSiteId: z.string().min(1, "Choose the site that does the work."),

  operationCode: z
    .string()
    .min(1, "Say what the site does — manufacture, package, test or release."),

  // Supplied rather than defaulted to today: an operation recorded now may
  // have run since 2019, and guessing would put a wrong date on a filing.
  effectiveFrom: z
    .string()
    .min(1, "Say when this site started performing the operation."),
});

export type ManufacturingOperationFormValues = z.infer<
  typeof manufacturingOperationSchema
>;
