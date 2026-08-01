import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface BeginMeetingBody {
  authorityId: string;
  subject: string;
  initialStatus: string;
  occurredOn: string;
  scheduledFor?: string | null;
}

export async function beginMeeting(
  body: BeginMeetingBody,
): Promise<{ id: string }> {
  const response = await apiFetch(buildUrl("/api/meetings"), {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to record this meeting."));
  }

  return response.json();
}
