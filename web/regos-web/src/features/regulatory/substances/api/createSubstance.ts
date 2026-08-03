import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

export interface CreateSubstanceBody {
  name: string;
  inn: string | null;
  substanceClassCode: string;
  substanceTypeCode: string;
  casNumber: string | null;
  uniiCode: string | null;
  molecularFormula: string | null;
  description: string | null;
}

/**
 * Adds a compound the shared catalogue does not carry.
 *
 * Nothing in the body says who owns it — the server takes that from the
 * session, so this call cannot ask for a shared substance (ADR-058 §5). A name
 * that already exists comes back as a business refusal, and its wording is
 * surfaced verbatim because it says *which* catalogue the clash is in, which is
 * what tells "use the one that is there" apart from "you added this already".
 */
export async function createSubstance(
  body: CreateSubstanceBody,
): Promise<{ id: string }> {
  const response = await apiFetch(buildUrl("/api/substances"), {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to add the substance."));
  }

  return response.json();
}
