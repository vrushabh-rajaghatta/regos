import { z } from "zod";

export const addMarketSchema = z.object({
  countryId: z.string().min(1, "A country is required."),

  // The business date the market presence began — supplied, never defaulted to
  // today, so a portfolio carried over from a legacy system can say when it
  // actually entered.
  statusDate: z.string().min(1, "A date is required."),
});

export type AddMarketFormValues = z.infer<typeof addMarketSchema>;
