import { buildUrl } from "@/shared/api/apiClient";
import type { ApplicationDetail } from "../types/ApplicationDetail";

export async function getApplication(
  applicationId: string
): Promise<ApplicationDetail> {
  const response = await fetch(buildUrl(`/applications/${applicationId}`));

  if (!response.ok) {
    throw new Error("Unable to load Application.");
  }

  return response.json();
}
