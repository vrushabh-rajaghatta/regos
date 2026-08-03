import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { StudyKind } from "../../studies";

/** A study this application says it rests on. */
export interface CitedStudy {
  studyId: string;
  kind: StudyKind;
  sponsorStudyIdentifier: string;
  title: string;
  citedOn: string;
}

/** "Which studies support this filing?" */
export async function listApplicationStudies(
  applicationId: string,
): Promise<CitedStudy[]> {
  const response = await apiFetch(
    buildUrl(`/api/applications/${applicationId}/studies`),
  );

  if (!response.ok) {
    throw new Error("Unable to load the studies this application cites.");
  }

  return response.json();
}
