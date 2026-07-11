import { z } from "zod";

export const registerProductSchema = z.object({
  name: z.string().trim().min(1, "Product name is required."),

  type: z.enum(["Drug", "MedicalDevice", "Biologic"]),
});

export type RegisterProductFormValues = z.infer<typeof registerProductSchema>;
