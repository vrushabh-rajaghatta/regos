import { apiFetch, buildUrl } from "@/shared/api/apiClient";

/**
 * A filing that names a study — the application itself, or one sequence whose
 * placements report it.
 */
export interface StudyFiling {
  kind: "Application" | "Sequence";
  applicationId: string;
  applicationName: string;
  applicationNumber: string | null;
  submissionId: string | null;
  submissionTitle: string | null;
  /** Formatted as eCTD writes it — `0000`. Null for a draft. */
  sequenceNumber: string | null;
}

/** "Which filings cite this study?" — the inverse of listApplicationStudies. */
export async function listStudyFilings(
  studyId: string,
): Promise<StudyFiling[]> {
  const response = await apiFetch(
    buildUrl(`/api/studies/${studyId}/filings`),
  );

  if (!response.ok) {
    throw new Error("Unable to load the filings that cite this study.");
  }

  return response.json();
}
