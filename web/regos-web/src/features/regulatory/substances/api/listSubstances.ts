import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { Substance, SubstanceOrigin } from "../types/Substance";

export interface ListSubstancesParams {
  search?: string;
  origin?: SubstanceOrigin;
}

/**
 * The shared catalogue and this organisation's own compounds, in one list.
 *
 * Search and the origin filter go to the server rather than being applied to a
 * fetched array: the catalogue grows with licensed terminology, and a filter
 * that only works while the list is small is a filter that breaks silently.
 */
export async function listSubstances(
  params: ListSubstancesParams = {},
): Promise<Substance[]> {
  const query = new URLSearchParams();

  if (params.search?.trim()) query.set("search", params.search.trim());
  if (params.origin && params.origin !== "Any") query.set("origin", params.origin);

  const suffix = query.toString() ? `?${query}` : "";

  const response = await apiFetch(buildUrl(`/api/substances${suffix}`));

  if (!response.ok) {
    throw new Error("Unable to load substances.");
  }

  return response.json();
}
