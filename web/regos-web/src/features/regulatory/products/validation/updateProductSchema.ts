import { z } from "zod";

export const updateProductSchema = z.object({
  name: z.string().trim().min(1, "Product name is required."),

  type: z.enum([
    "MedicalDevice",
    "Drug",
    "Biologic",
    "CombinationProduct",
    "IVD",
  ]),
});

export type UpdateProductFormValues = z.infer<typeof updateProductSchema>;
