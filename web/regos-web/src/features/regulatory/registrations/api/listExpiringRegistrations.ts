import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import type { ExpiringRegistration } from "../types/ExpiringRegistration";

export async function listExpiringRegistrations(): Promise<
  ExpiringRegistration[]
> {
  const response = await apiFetch(buildUrl("/api/registrations/expiring"));

  if (!response.ok) {
    throw new Error("Unable to load expiring registrations.");
  }

  return response.json();
}
