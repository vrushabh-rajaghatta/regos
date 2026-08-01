import { z } from "zod";

export const beginInspectionSchema = z.object({
  authorityId: z.string().min(1, "A health authority is required."),

  title: z
    .string()
    .min(1, "A title is required.")
    .max(300, "A title cannot exceed 300 characters."),

  initialStatus: z.enum(["Announced", "InProgress"], {
    message: "Say whether they announced it or arrived.",
  }),

  occurredOn: z.string().min(1, "A date is required."),

  // What was inspected. Often unknown when the notice arrives.
  organizationSiteId: z.string().optional(),

  scheduledFor: z.string().optional(),
});

export type BeginInspectionFormValues = z.infer<typeof beginInspectionSchema>;
