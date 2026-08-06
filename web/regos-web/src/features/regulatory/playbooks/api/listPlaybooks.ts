import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { PlaybookSummary } from "../types/PlaybookSummary";

export async function listPlaybooks(): Promise<PlaybookSummary[]> {
  const response = await apiFetch(buildUrl("/process-definitions"));

  if (!response.ok) {
    throw new Error("Unable to load playbooks.");
  }

  return response.json();
}
