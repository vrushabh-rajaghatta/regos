import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { SessionSummary } from "../types/SessionSummary";

export async function getSessions(): Promise<SessionSummary[]> {
  const response = await apiFetch(buildUrl("/api/auth/sessions"));

  if (!response.ok) throw new Error("Unable to load your sessions.");

  return response.json();
}

export async function revokeSession(sessionId: string): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/auth/sessions/${sessionId}`),
    { method: "DELETE" },
  );

  if (!response.ok) throw new Error("Unable to end that session.");
}

export async function revokeOtherSessions(): Promise<void> {
  const response = await apiFetch(
    buildUrl("/api/auth/sessions/revoke-others"),
    { method: "POST" },
  );

  if (!response.ok) throw new Error("Unable to end your other sessions.");
}
