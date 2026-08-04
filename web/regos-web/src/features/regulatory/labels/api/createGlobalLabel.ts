import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface CreateGlobalLabelBody {
  name: string;
  labelTypeCode: string;
}

/**
 * Starts holding a label for a product. The first draft is opened with it — a
 * label with no version is a name with nothing behind it — and its id comes
 * back so content can be attached without a second round trip.
 */
export async function createGlobalLabel(
  globalProductId: string,
  body: CreateGlobalLabelBody,
): Promise<{ id: string; draftVersionId: string }> {
  const response = await apiFetch(
    buildUrl(`/api/products/${globalProductId}/global-labels`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to add the label."));
  }

  return response.json();
}
