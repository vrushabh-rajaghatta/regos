import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import type { AttachableProductDocument } from "../types/AttachableProductDocument";

export async function listAttachableProductDocuments(
  submissionId: string
): Promise<AttachableProductDocument[]> {
  const response = await apiFetch(
    buildUrl(`/submissions/${submissionId}/attachable-documents`)
  );

  if (!response.ok) {
    throw new Error("Unable to load available documents.");
  }

  return response.json();
}
