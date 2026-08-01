import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export async function removeCorrespondenceContent(
  correspondenceId: string,
  attachmentId: string,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/correspondence/${correspondenceId}/content/${attachmentId}`),
    { method: "DELETE" },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to remove this file."));
  }
}
