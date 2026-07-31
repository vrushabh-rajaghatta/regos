import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { ExpiringRegistration } from "../types/ExpiringRegistration";
import type { MarketRegistrationSummary } from "../types/MarketRegistrationSummary";
import type { RegistrationDetail } from "../types/RegistrationDetail";
import type { RegistrationMarket } from "../types/RegistrationMarket";
import type { RegistrationSummary } from "../types/RegistrationSummary";

/**
 * The API surfaces the server's own words when it refuses something — the
 * lifecycle messages are written to be read by a regulatory user ("A Withdrawn
 * registration has reached the end of its lifecycle"), and paraphrasing them
 * here would mean maintaining a second copy of the domain's vocabulary.
 */
async function detailOf(response: Response, fallback: string): Promise<string> {
  try {
    const problem = await response.json();
    return typeof problem?.detail === "string" ? problem.detail : fallback;
  } catch {
    return fallback;
  }
}

export async function listProductRegistrations(
  productId: string
): Promise<RegistrationSummary[]> {
  const response = await apiFetch(
    buildUrl(`/api/products/${productId}/registrations`)
  );

  if (!response.ok) {
    throw new Error("Unable to load this product's registrations.");
  }

  return response.json();
}

export async function listMarketRegistrations(
  countryId: string
): Promise<MarketRegistrationSummary[]> {
  const response = await apiFetch(
    buildUrl(`/api/countries/${countryId}/registrations`)
  );

  if (!response.ok) {
    throw new Error("Unable to load this market's registrations.");
  }

  return response.json();
}

export async function listRegistrationMarkets(): Promise<RegistrationMarket[]> {
  const response = await apiFetch(buildUrl("/api/registrations/markets"));

  if (!response.ok) {
    throw new Error("Unable to load the markets.");
  }

  return response.json();
}

export async function listExpiringRegistrations(): Promise<
  ExpiringRegistration[]
> {
  const response = await apiFetch(buildUrl("/api/registrations/expiring"));

  if (!response.ok) {
    throw new Error("Unable to load expiring registrations.");
  }

  return response.json();
}

export async function getRegistration(
  registrationId: string
): Promise<RegistrationDetail> {
  const response = await apiFetch(
    buildUrl(`/registrations/${registrationId}`)
  );

  if (!response.ok) {
    throw new Error("Unable to load this registration.");
  }

  return response.json();
}

export interface CreateRegistrationBody {
  countryId: string;
  authorityId: string;
  holderOrganizationId: string;
  occurredOn: string;
  originatingApplicationId?: string | null;
  note?: string | null;
}

export async function createRegistration(
  productId: string,
  body: CreateRegistrationBody
): Promise<{ id: string }> {
  const response = await apiFetch(
    buildUrl(`/api/products/${productId}/registrations`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    }
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to create the registration.")
    );
  }

  return response.json();
}

export interface RecordApprovalBody {
  registrationNumber: string;
  approvedOn: string;
  expiresOn?: string | null;
  note?: string | null;
}

export async function recordRegistrationApproval(
  registrationId: string,
  body: RecordApprovalBody
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/registrations/${registrationId}/approval`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    }
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to record the approval."));
  }
}

export interface ChangeStatusBody {
  status: string;
  occurredOn: string;
  note?: string | null;
}

export async function changeRegistrationStatus(
  registrationId: string,
  body: ChangeStatusBody
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/registrations/${registrationId}/status`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    }
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to change the status."));
  }
}
