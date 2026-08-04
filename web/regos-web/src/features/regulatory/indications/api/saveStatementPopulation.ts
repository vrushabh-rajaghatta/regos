import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

import type { PopulationBody } from "../types/Indication";
import type { StatementKind } from "../types/StatementKind";

/**
 * Adds a qualifier, or **amends one in place**.
 *
 * The amend is a PUT on the population's own id, not a replace of the
 * collection — a band written as 2–12 and corrected to 2–11 is the same
 * qualifier, and the id survives the correction (EPIC-018 D2).
 */
export async function saveStatementPopulation(
  kind: StatementKind,
  statementId: string,
  populationId: string | null,
  body: PopulationBody,
): Promise<void> {
  const path = populationId
    ? `/api/${kind}/${statementId}/populations/${populationId}`
    : `/api/${kind}/${statementId}/populations`;

  const response = await apiFetch(buildUrl(path), {
    method: populationId ? "PUT" : "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to save the population."));
  }
}
