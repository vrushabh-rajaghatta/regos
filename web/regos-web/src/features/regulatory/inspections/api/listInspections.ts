import { apiFetch, buildUrl } from "@/shared/api/apiClient";

export interface InspectionHistoryEntry {
  status: string;
  occurredOn: string;
  recordedOnUtc: string;
  note: string | null;
}

export interface Inspection {
  inspectionId: string;
  title: string;
  authorityId: string;
  authorityName: string;
  organizationSiteId: string | null;
  organizationSiteName: string | null;
  raisedOn: string;
  scheduledFor: string | null;
  completedOn: string | null;
  currentStatus: string;
  outcome: string | null;
  history: InspectionHistoryEntry[];
}

export async function listInspections(
  includeConcluded: boolean,
): Promise<Inspection[]> {
  const response = await apiFetch(
    buildUrl(`/api/inspections${includeConcluded ? "?includeConcluded=true" : ""}`),
  );

  if (!response.ok) throw new Error("Unable to load inspections.");

  return response.json();
}
