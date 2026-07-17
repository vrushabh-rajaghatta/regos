import { buildUrl } from "@/shared/api/apiClient";
import type { DocumentTypeOption } from "../types/DocumentTypeOption";

export async function listDocumentTypes(): Promise<DocumentTypeOption[]> {
  const response = await fetch(buildUrl("/reference-data/document-types"));

  if (!response.ok) {
    throw new Error("Unable to load Document Types.");
  }

  return response.json();
}
