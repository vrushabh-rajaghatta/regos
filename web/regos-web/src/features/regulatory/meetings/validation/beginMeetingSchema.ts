import { z } from "zod";

export const beginMeetingSchema = z.object({
  authorityId: z.string().min(1, "A health authority is required."),

  subject: z
    .string()
    .min(1, "A subject is required.")
    .max(300, "A subject cannot exceed 300 characters."),

  // Two different business events, so the caller says which.
  initialStatus: z.enum(["Requested", "Granted"], {
    message: "Say whether we asked for it or they called it.",
  }),

  occurredOn: z.string().min(1, "A date is required."),

  scheduledFor: z.string().optional(),
});

export type BeginMeetingFormValues = z.infer<typeof beginMeetingSchema>;
