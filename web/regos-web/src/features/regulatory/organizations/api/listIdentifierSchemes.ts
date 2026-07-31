import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { IdentifierScheme } from "../types/IdentifierScheme";

export async function listIdentifierSchemes(): Promise<IdentifierScheme[]> {
  const response = await apiFetch(
    buildUrl("/api/reference-data/identifier-schemes"),
  );

  if (!response.ok) {
    throw new Error("Unable to load identifier schemes.");
  }

  return response.json();
}
