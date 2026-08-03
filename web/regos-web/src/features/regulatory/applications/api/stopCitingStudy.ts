import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

/** Withdraws a citation. The study is named in the path; an application cites it once or not at all. */
export async function stopCitingStudy(
  applicationId: string,
  studyId: string,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/applications/${applicationId}/studies/${studyId}`),
    { method: "DELETE" },
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to withdraw the citation."),
    );
  }
}
