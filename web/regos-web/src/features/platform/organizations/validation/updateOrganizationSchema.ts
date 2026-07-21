import { z } from "zod";

export const updateOrganizationSchema = z.object({

  legalName: z.string().trim().min(1, "Legal name is required."),

  type: z.string().trim().min(1, "Organization type is required."),
});

export type UpdateOrganizationFormValues = z.infer<
  typeof updateOrganizationSchema
>;
