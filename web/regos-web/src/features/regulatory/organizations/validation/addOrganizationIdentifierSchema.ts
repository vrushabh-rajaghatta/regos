import { z } from "zod";

export const addOrganizationIdentifierSchema = z.object({

  schemeId: z.string().trim().min(1, "Choose an identifier scheme."),

  // The server enforces the length ceiling and the one-per-scheme rule; this
  // only catches an empty submit, which is not worth a round trip.
  value: z.string().trim().min(1, "Identifier value is required."),
});

export type AddOrganizationIdentifierFormValues = z.infer<
  typeof addOrganizationIdentifierSchema
>;
