import { z } from "zod";

/**
 * The password rules the API enforces, repeated here so the user learns about a
 * short password before submitting rather than after. The server remains the
 * authority — this only saves a round trip, and the confirmation field exists
 * only in the browser, since the API has nothing to compare it against.
 */
export const acceptInvitationSchema = z
  .object({
    password: z
      .string()
      .min(8, "Password must be at least 8 characters.")
      .max(256, "Password must be at most 256 characters."),

    confirmPassword: z.string().min(1, "Please confirm your password."),
  })
  .refine((values) => values.password === values.confirmPassword, {
    message: "Passwords do not match.",
    path: ["confirmPassword"],
  });

export type AcceptInvitationFormValues = z.infer<
  typeof acceptInvitationSchema
>;
