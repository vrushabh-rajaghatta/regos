import { apiFetch, buildUrl } from "@/shared/api/apiClient";

export interface DueWorkItem {
  kind: string;
  id: string;
  correspondenceId: string | null;
  title: string;
  authorityName: string;
  dueOn: string | null;
  ownerUserId: string | null;
  status: string;
}

export async function listDueWork(
  mine: boolean,
  dueOnOrBefore?: string,
): Promise<DueWorkItem[]> {
  const params = new URLSearchParams();
  if (mine) params.set("mine", "true");
  if (dueOnOrBefore) params.set("dueOnOrBefore", dueOnOrBefore);

  const query = params.toString();

  const response = await apiFetch(
    buildUrl(`/api/due-work${query ? `?${query}` : ""}`),
  );

  if (!response.ok) {
    throw new Error("Unable to load what is due.");
  }

  return response.json();
}
