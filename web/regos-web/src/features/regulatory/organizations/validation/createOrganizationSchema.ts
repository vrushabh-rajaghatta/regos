import { z } from "zod";

export const createOrganizationSchema = z.object({

  legalName: z.string().trim().min(1, "Legal name is required."),

  type: z.string().trim().min(1, "Organization type is required."),
});

export type CreateOrganizationFormValues = z.infer<
  typeof createOrganizationSchema
>;
