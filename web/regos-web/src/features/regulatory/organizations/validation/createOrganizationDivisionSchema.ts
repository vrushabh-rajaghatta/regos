import { z } from "zod";

export const createOrganizationDivisionSchema = z.object({

  name: z.string().trim().min(1, "Division name is required."),

  // Required here and optional on the organization itself. The asymmetry is
  // deliberate and recorded in the aggregate: Organization.Create predates the
  // field, while a division has always had to say when it was established.
  statusDate: z.string().trim().min(1, "Established date is required."),

  acronym: z.string().trim().max(50, "Acronym is too long.").optional(),
});

export type CreateOrganizationDivisionFormValues = z.infer<
  typeof createOrganizationDivisionSchema
>;
