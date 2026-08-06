import { apiFetch, buildUrl } from "@/shared/api/apiClient";

export interface CreateObjectiveRequest {
  globalProductId: string;
  countryId: string;
  name: string;
  statedOn: string;
  rationale: string | null;
  ownerUserId: string | null;
  targetCompletionOn: string | null;
}

/**
 * The request carries no status. A new objective is always Proposed — deciding
 * to pursue it is a second, dated event.
 */
export async function createObjective(
  request: CreateObjectiveRequest,
): Promise<{ id: string }> {
  const response = await apiFetch(buildUrl("/process-objectives"), {
    method: "POST",
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const problem = await response.json().catch(() => null);

    throw new Error(problem?.detail ?? "Unable to state the objective.");
  }

  return response.json();
}
