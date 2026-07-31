import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import type { RegistrationDetail } from "../types/RegistrationDetail";

export async function getRegistration(
  registrationId: string
): Promise<RegistrationDetail> {
  const response = await apiFetch(
    buildUrl(`/registrations/${registrationId}`)
  );

  if (!response.ok) {
    throw new Error("Unable to load this registration.");
  }

  return response.json();
}
