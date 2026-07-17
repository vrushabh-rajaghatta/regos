import { z } from "zod";

export const uploadProductDocumentSchema = z.object({
  documentTypeId: z.string().min(1, "Document Type is required."),

  name: z.string().trim().min(1, "Document name is required."),
});

export type UploadProductDocumentFormValues = z.infer<
  typeof uploadProductDocumentSchema
>;
