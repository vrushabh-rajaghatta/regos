import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { RegulatoryTemplateSummary } from "../types/RegulatoryTemplateSummary";

export async function listRegulatoryTemplates(): Promise<
  RegulatoryTemplateSummary[]
> {
  const response = await apiFetch(buildUrl("/reference-data/templates"));

  if (!response.ok) {
    throw new Error("Unable to load regulatory templates.");
  }

  return response.json();
}
