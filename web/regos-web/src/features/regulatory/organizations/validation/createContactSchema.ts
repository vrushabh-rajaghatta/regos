import { z } from "zod";

export const createContactSchema = z.object({

  firstName: z.string().trim().min(1, "First name is required."),

  lastName: z.string().trim().min(1, "Last name is required."),

  statusDate: z.string().trim().min(1, "Appointed date is required."),

  title: z.string().trim().optional(),

  department: z.string().trim().optional(),

  // Optional: a head-office regulatory lead has no site. Empty means "not at a
  // specific site", not "invalid".
  organizationSiteId: z.string().trim().optional(),

  roleId: z.string().trim().optional(),

  // The server requires an "@" and nothing more — it stores what the registry
  // was told, and a stricter client rule would reject addresses the API accepts.
  email: z
    .string()
    .trim()
    .refine((value) => value === "" || value.includes("@"), {
      message: "Enter an email address, or leave it blank.",
    })
    .optional(),

  phone: z.string().trim().optional(),
});

export type CreateContactFormValues = z.infer<typeof createContactSchema>;
