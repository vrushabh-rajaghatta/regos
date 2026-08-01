import { z } from "zod";

export const changeMarketStatusSchema = z.object({
  status: z.enum(["Launched", "TemporarilyUnavailable", "Discontinued"]),

  // The business date this became true — never defaulted to today, so a
  // portfolio carried over from a legacy system can say when it happened.
  occurredOn: z.string().min(1, "A date is required."),

  note: z.string().trim().max(500).optional(),
});

export type ChangeMarketStatusFormValues = z.infer<
  typeof changeMarketStatusSchema
>;
