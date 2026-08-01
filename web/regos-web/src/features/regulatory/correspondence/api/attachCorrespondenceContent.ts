import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export async function attachCorrespondenceContent(
  correspondenceId: string,
  file: File,
): Promise<{ id: string }> {
  const body = new FormData();
  body.append("file", file);

  const response = await apiFetch(
    buildUrl(`/api/correspondence/${correspondenceId}/content`),
    { method: "POST", body },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to attach this file."));
  }

  return response.json();
}
