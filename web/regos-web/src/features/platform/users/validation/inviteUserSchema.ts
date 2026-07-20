import { z } from "zod";

export const inviteUserSchema = z.object({

  firstName: z.string().trim().min(1, "First name is required."),

  lastName: z.string().trim().min(1, "Last name is required."),

  email: z
    .string()
    .trim()
    .min(1, "Email is required.")
    .email("Enter a valid email address."),
});

export type InviteUserFormValues = z.infer<typeof inviteUserSchema>;
