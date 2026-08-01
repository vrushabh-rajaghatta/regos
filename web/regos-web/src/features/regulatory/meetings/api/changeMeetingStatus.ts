import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export async function changeMeetingStatus(
  meetingId: string,
  status: string,
  occurredOn: string,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/meetings/${meetingId}/status`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ status, occurredOn }),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to change this meeting."));
  }
}
