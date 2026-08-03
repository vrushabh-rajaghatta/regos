import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

import type { StudyKind } from "../types/Study";

export interface RegisterStudyBody {
  sponsorStudyIdentifier: string;
  title: string;
}

/**
 * Two routes, one function: the kind selects the route rather than travelling
 * in the body, because a clinical and a non-clinical study are different
 * aggregates (ADR-056) and a `kind` field on the wire is where a discriminator
 * in the domain would start.
 *
 * A duplicate identifier comes back as a business refusal, and its `detail` is
 * surfaced verbatim — it names the study already using the code, which is what
 * tells a typo apart from a duplicate.
 */
export async function registerStudy(
  kind: StudyKind,
  body: RegisterStudyBody,
): Promise<{ id: string }> {
  const path = kind === "Clinical" ? "clinical" : "nonclinical";

  const response = await apiFetch(buildUrl(`/api/studies/${path}`), {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to register the study."));
  }

  return response.json();
}
