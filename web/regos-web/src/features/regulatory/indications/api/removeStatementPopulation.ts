import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

import type { StatementKind } from "../types/StatementKind";

/** Removes a qualifier recorded in error. */
export async function removeStatementPopulation(
  kind: StatementKind,
  statementId: string,
  populationId: string,
): Promise<void> {
  const response = await apiFetch(
    buildUrl(`/api/${kind}/${statementId}/populations/${populationId}`),
    { method: "DELETE" },
  );

  if (!response.ok) {
    throw new Error(
      await detailOf(response, "Unable to remove the population."),
    );
  }
}
