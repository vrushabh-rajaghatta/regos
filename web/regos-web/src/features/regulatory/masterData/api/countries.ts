import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import type { CountryDto } from "../types/CountryDto";

export async function listCountries(): Promise<CountryDto[]> {
  const response = await apiFetch(buildUrl("/master-data/countries"));

  if (!response.ok) {
    throw new Error("Unable to load countries.");
  }

  return response.json();
}
