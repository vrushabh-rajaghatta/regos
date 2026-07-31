import { z } from "zod";

export const createOrganizationSiteSchema = z.object({

  name: z.string().trim().min(1, "Site name is required."),

  type: z.string().trim().min(1, "Site type is required."),

  // Country is the one address field the server requires: a site's country is
  // what a regulator files it under, while the street is correspondence detail.
  countryId: z.string().trim().min(1, "Country is required."),

  statusDate: z.string().trim().min(1, "Opened date is required."),

  addressLine1: z.string().trim().optional(),

  city: z.string().trim().optional(),

  postalCode: z.string().trim().optional(),
});

export type CreateOrganizationSiteFormValues = z.infer<
  typeof createOrganizationSiteSchema
>;
