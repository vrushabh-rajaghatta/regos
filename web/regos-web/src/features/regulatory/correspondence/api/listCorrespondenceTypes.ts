import { apiFetch, buildUrl } from "@/shared/api/apiClient";

export interface CorrespondenceType {
  id: string;
  code: string;
  name: string;
}

export async function listCorrespondenceTypes(): Promise<CorrespondenceType[]> {
  const response = await apiFetch(
    buildUrl("/api/master-data/correspondence-types"),
  );

  if (!response.ok) {
    throw new Error("Unable to load correspondence types.");
  }

  return response.json();
}
