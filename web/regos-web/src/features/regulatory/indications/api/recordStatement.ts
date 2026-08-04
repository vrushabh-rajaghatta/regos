import { apiFetch, buildUrl } from "@/shared/api/apiClient";
import { detailOf } from "@/shared/api/problemDetail";

import type { StatementKind } from "../types/StatementKind";

export interface RecordStatementBody {
  conditionCode: string;
  labelText: string;
  /** Undesirable effects only; the other two ignore it. */
  frequencyCode?: string | null;
}

/**
 * Records a statement inside this market's approved label.
 *
 * **No approval date, unlike an indication.** A contraindication and an
 * undesirable effect are content within a label the authority approved — what
 * changes them is a new label revision, not a decision recorded here.
 */
export async function recordStatement(
  kind: StatementKind,
  medicinalProductId: string,
  body: RecordStatementBody,
): Promise<{ id: string }> {
  const response = await apiFetch(
    buildUrl(`/api/medicinal-products/${medicinalProductId}/${kind}`),
    {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
    },
  );

  if (!response.ok) {
    throw new Error(await detailOf(response, "Unable to record the statement."));
  }

  return response.json();
}
