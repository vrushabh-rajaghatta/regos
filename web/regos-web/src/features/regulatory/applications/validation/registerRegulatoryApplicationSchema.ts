import { z } from "zod";

export const registerRegulatoryApplicationSchema = z.object({
  name: z.string().trim().min(1, "Application name is required."),

  countryId: z.string().min(1, "Country is required."),

  authorityId: z.string().min(1, "Authority is required."),

  applicationTypeId: z.string().min(1, "Application type is required."),

  applicantOrganizationId: z
    .string()
    .min(1, "Applicant organization is required."),
});

export type RegisterRegulatoryApplicationFormValues = z.infer<
  typeof registerRegulatoryApplicationSchema
>;
