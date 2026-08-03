import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

import type { StudyKind } from "../../studies";

/**
 * Records which study a placement reports — or, with a null study, that it
 * reports none.
 *
 * Two typed fields on the wire rather than a `(kind, id)` pair: a clinical and
 * a non-clinical study are different aggregates (ADR-056), and a discriminator
 * here is where one in the domain would start. The request states the whole
 * fact, so sending it twice lands in the same place.
 */
export async function reportStudyOnPlacement(
  submissionId: string,
  submissionDocumentId: string,
  study: { id: string; kind: StudyKind } | null,
  fileTag: string | null,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(
      `/api/submissions/${submissionId}/documents/${submissionDocumentId}/study`,
    ),
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        clinicalStudyId: study?.kind === "Clinical" ? study.id : null,
        nonClinicalStudyId: study?.kind === "NonClinical" ? study.id : null,
        // Sent together, because they are one fact: which study, in what role.
        fileTag: study ? fileTag : null,
      }),
    },
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to record the study."),
    );
  }
}
