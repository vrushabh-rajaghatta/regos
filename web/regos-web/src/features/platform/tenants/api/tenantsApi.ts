import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { CreateTenantRequest } from "../types/CreateTenantRequest";
import type { TenantSummary } from "../types/TenantSummary";

/**
 * Platform administration (ADR-033): every call here needs the
 * PlatformAdministrator role; anyone else gets the API's 403.
 */
export async function listTenants(): Promise<TenantSummary[]> {
  const response = await apiFetch(buildUrl("/api/platform/tenants"));

  if (!response.ok) {
    throw new Error("Unable to load tenants.");
  }

  return response.json();
}

export async function createTenant(
  request: CreateTenantRequest,
): Promise<{ tenantId: string; adminUserId: string }> {
  const response = await apiFetch(buildUrl("/api/platform/tenants"), {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    // Surface the API's ProblemDetails message so the platform admin sees why
    // provisioning was rejected (name missing, email already in use, ...).
    let message = "Unable to create the tenant.";

    try {
      const problem = await response.json();

      if (typeof problem?.detail === "string") {
        message = problem.detail;
      }
    } catch {
      // No problem body — fall back to the generic message.
    }

    throw new Error(message);
  }

  return response.json();
}

export async function activateTenant(tenantId: string): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/platform/tenants/${tenantId}/activate`),
    { method: "POST" },
  );

  if (!response.ok) {
    throw new Error("Unable to activate the tenant.");
  }
}

export async function deactivateTenant(tenantId: string): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/platform/tenants/${tenantId}/deactivate`),
    { method: "POST" },
  );

  if (!response.ok) {
    throw new Error("Unable to deactivate the tenant.");
  }
}
