import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

import type { StudyKind } from "../../studies";

/**
 * Records that a study supports this application.
 *
 * The kind selects which typed field carries the id rather than travelling as a
 * discriminator, for the reason ADR-056 §2 gives.
 */
export async function citeStudy(
  applicationId: string,
  study: { id: string; kind: StudyKind },
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/applications/${applicationId}/studies`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        clinicalStudyId: study.kind === "Clinical" ? study.id : null,
        nonClinicalStudyId: study.kind === "NonClinical" ? study.id : null,
      }),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to cite the study."));
  }
}
