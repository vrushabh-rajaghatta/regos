import { z } from "zod";

/**
 * Only enough to catch an obvious typo before a round trip. Note what is
 * deliberately absent: no check that the address is one RegOS knows, because
 * the browser must not be able to answer that question either.
 */
export const forgotPasswordSchema = z.object({
  email: z
    .string()
    .min(1, "Email address is required.")
    .email("Enter a valid email address."),
});

export type ForgotPasswordFormValues = z.infer<typeof forgotPasswordSchema>;

/**
 * The password rules the API enforces, repeated here so the user learns about a
 * short password before submitting rather than after. The server remains the
 * authority — this only saves a round trip, and the confirmation field exists
 * only in the browser, since the API has nothing to compare it against.
 */
export const resetPasswordSchema = z
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

export type ResetPasswordFormValues = z.infer<typeof resetPasswordSchema>;
