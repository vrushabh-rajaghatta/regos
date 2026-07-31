import { apiFetch, buildUrl } from "@/shared/api/apiClient";

export interface ContactRoleOption {
  id: string;
  code: string;
  name: string;
  description: string | null;
  isTenantOwn: boolean;
}

export async function listContactRoles(): Promise<ContactRoleOption[]> {
  const response = await apiFetch(
    buildUrl("/api/reference-data/contact-roles"),
  );

  if (!response.ok) {
    throw new Error("Unable to load contact roles.");
  }

  return response.json();
}
