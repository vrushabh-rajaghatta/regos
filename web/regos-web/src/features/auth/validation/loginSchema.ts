import { z } from "zod";

/**
 * Presence only. The browser deliberately does not validate the shape of the
 * email or the strength of the password: the API answers every bad sign-in
 * identically, and a client-side rule that rejected input before sending it
 * would tell an attacker something the server refuses to.
 */
export const loginSchema = z.object({
  email: z.string().trim().min(1, "Email address is required."),

  password: z.string().min(1, "Password is required."),
});

export type LoginFormValues = z.infer<typeof loginSchema>;
