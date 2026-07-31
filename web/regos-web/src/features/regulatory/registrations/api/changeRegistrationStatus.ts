import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "./problemDetail";

export interface ChangeStatusBody {
  status: string;
  occurredOn: string;
  note?: string | null;
}

export async function changeRegistrationStatus(
  registrationId: string,
  body: ChangeStatusBody
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/registrations/${registrationId}/status`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    }
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to change the status."));
  }
}
