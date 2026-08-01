import { z } from "zod";

export const marketActivationSchema = z.object({
  // The business date the record left or returned to normal work — supplied,
  // never taken from the clock, like every other date in this context.
  on: z.string().min(1, "A date is required."),
});

export type MarketActivationFormValues = z.infer<typeof marketActivationSchema>;
