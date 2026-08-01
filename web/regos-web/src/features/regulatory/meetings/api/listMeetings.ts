import { apiFetch, buildUrl } from "@/shared/api/apiClient";

export interface MeetingHistoryEntry {
  status: string;
  occurredOn: string;
  recordedOnUtc: string;
  note: string | null;
}

export interface Meeting {
  meetingId: string;
  subject: string;
  authorityId: string;
  authorityName: string;
  authorityDivisionName: string | null;
  raisedOn: string;
  scheduledFor: string | null;
  heldOn: string | null;
  currentStatus: string;
  minutes: string | null;
  outcome: string | null;
  history: MeetingHistoryEntry[];
}

export async function listMeetings(includeConcluded: boolean): Promise<Meeting[]> {
  const response = await apiFetch(
    buildUrl(`/api/meetings${includeConcluded ? "?includeConcluded=true" : ""}`),
  );

  if (!response.ok) throw new Error("Unable to load meetings.");

  return response.json();
}
