import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface ChangeMarketStatusBody {
  status: string;
  occurredOn: string;
  note?: string | null;
}

export async function changeMarketStatus(
  medicinalProductId: string,
  body: ChangeMarketStatusBody,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/medicinal-products/${medicinalProductId}/market-status`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to record the market status."),
    );
  }
}
