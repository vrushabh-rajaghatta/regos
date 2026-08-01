import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { Correspondence } from "../types/Correspondence";

export interface ListCorrespondenceFilters {
  authorityId?: string;
  correspondenceTypeId?: string;
  direction?: string;
  regulatoryApplicationId?: string;
}

export async function listCorrespondence(
  filters: ListCorrespondenceFilters = {},
): Promise<Correspondence[]> {
  const params = new URLSearchParams();

  if (filters.authorityId) params.set("authorityId", filters.authorityId);
  if (filters.correspondenceTypeId)
    params.set("correspondenceTypeId", filters.correspondenceTypeId);
  if (filters.direction) params.set("direction", filters.direction);
  if (filters.regulatoryApplicationId)
    params.set("regulatoryApplicationId", filters.regulatoryApplicationId);

  const query = params.toString();

  const response = await apiFetch(
    buildUrl(`/api/correspondence${query ? `?${query}` : ""}`),
  );

  if (!response.ok) {
    throw new Error("Unable to load correspondence.");
  }

  return response.json();
}
