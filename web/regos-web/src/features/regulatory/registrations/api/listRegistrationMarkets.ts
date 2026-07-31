import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import type { RegistrationMarket } from "../types/RegistrationMarket";

export async function listRegistrationMarkets(): Promise<RegistrationMarket[]> {
  const response = await apiFetch(buildUrl("/api/registrations/markets"));

  if (!response.ok) {
    throw new Error("Unable to load the markets.");
  }

  return response.json();
}
