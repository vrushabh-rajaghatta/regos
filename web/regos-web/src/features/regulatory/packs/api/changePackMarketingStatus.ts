import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface ChangePackMarketingStatusBody {
  status: string;
  occurredOn: string;
  note?: string | null;
}

export async function changePackMarketingStatus(
  packagedProductId: string,
  body: ChangePackMarketingStatusBody,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/packaged-products/${packagedProductId}/marketing-status`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to record the pack's status."),
    );
  }
}
