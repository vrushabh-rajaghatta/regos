import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { RegulatoryTemplateDetail } from "../types/RegulatoryTemplateDetail";

export async function getRegulatoryTemplate(
  id: string,
): Promise<RegulatoryTemplateDetail> {
  const response = await apiFetch(buildUrl(`/reference-data/templates/${id}`));

  if (!response.ok) {
    throw new Error("Unable to load the regulatory template.");
  }

  return response.json();
}
