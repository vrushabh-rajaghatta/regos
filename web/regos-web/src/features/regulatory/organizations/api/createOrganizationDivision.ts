import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import { detailOf } from "./problemDetail";

export interface CreateOrganizationDivisionRequest {
  name: string;
  statusDate: string;
  acronym: string | null;
}

export async function createOrganizationDivision(
  organizationId: string,
  request: CreateOrganizationDivisionRequest,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/organizations/${organizationId}/divisions`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(request),
    },
  );

  if (response.ok) return;

  throw new Error(await detailOf(response, "Unable to record this division."));
}
