import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface RecordApprovalBody {
  registrationNumber: string;
  approvedOn: string;
  expiresOn?: string | null;
  note?: string | null;
}

export async function recordRegistrationApproval(
  registrationId: string,
  body: RecordApprovalBody
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/registrations/${registrationId}/approval`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    }
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to record the approval."));
  }
}
