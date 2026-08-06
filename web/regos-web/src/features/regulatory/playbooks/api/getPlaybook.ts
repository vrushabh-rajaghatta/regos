import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { PlaybookDetail } from "../types/PlaybookDetail";

export async function getPlaybook(
  id: string,
  version?: number,
): Promise<PlaybookDetail> {
  const query = version === undefined ? "" : `?version=${version}`;
  const response = await apiFetch(buildUrl(`/process-definitions/${id}${query}`));

  if (!response.ok) {
    throw new Error("Unable to load the playbook.");
  }

  return response.json();
}
