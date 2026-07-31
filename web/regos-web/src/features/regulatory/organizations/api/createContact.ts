import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import { detailOf } from "./problemDetail";

export interface CreateContactRequest {
  firstName: string;
  lastName: string;
  statusDate: string;
  organizationSiteId?: string | null;
  title?: string | null;
  department?: string | null;
  countryId?: string | null;
  roleIds?: string[];
  emails?: string[];
  phones?: string[];
}

export async function createContact(
  organizationId: string,
  request: CreateContactRequest,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/organizations/${organizationId}/contacts`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(request),
    },
  );

  if (response.ok) return;

  throw new Error(await detailOf(response, "Unable to record this contact."));
}
