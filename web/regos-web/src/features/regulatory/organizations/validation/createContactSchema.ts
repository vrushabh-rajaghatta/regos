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

  // Office, fax or mobile. Empty is legal and means "not said" — the server
  // stores that as a null, which is a different thing from a wrong guess.
  phoneKind: z.enum(["Business", "Fax", "Mobile"]).or(z.literal("")).optional(),
})
  // A kind without a number describes nothing. The reverse is fine: a number
  // whose kind nobody supplied is exactly what the nullable column is for.
  .refine((values) => !values.phoneKind || !!values.phone, {
    message: "Enter a phone number, or clear the type.",
    path: ["phone"],
  });

export type CreateContactFormValues = z.infer<typeof createContactSchema>;
