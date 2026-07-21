import { z } from "zod";

/**
 * The API's password rules, repeated so a short password is caught before a
 * round trip. Note what is deliberately absent: any rule that the new password
 * must differ from the current one. The API does not enforce that, and a
 * browser that did would be inventing a policy nobody decided.
 */
export const changePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, "Enter your current password."),

    newPassword: z
      .string()
      .min(8, "Password must be at least 8 characters.")
      .max(256, "Password must be at most 256 characters."),

    confirmPassword: z.string().min(1, "Please confirm your new password."),
  })
  .refine((values) => values.newPassword === values.confirmPassword, {
    message: "Passwords do not match.",
    path: ["confirmPassword"],
  });

export type ChangePasswordFormValues = z.infer<typeof changePasswordSchema>;
