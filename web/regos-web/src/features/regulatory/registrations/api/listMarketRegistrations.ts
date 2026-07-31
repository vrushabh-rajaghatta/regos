import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import type { MarketRegistrationSummary } from "../types/MarketRegistrationSummary";

export async function listMarketRegistrations(
  countryId: string
): Promise<MarketRegistrationSummary[]> {
  const response = await apiFetch(
    buildUrl(`/api/countries/${countryId}/registrations`)
  );

  if (!response.ok) {
    throw new Error("Unable to load this market's registrations.");
  }

  return response.json();
}
