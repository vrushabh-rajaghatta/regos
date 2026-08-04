import { apiFetch, buildUrl } from "@/shared/api/apiClient";

import type { CodedValue } from "../types/Presentation";

/**
 * The units a strength may be measured in — mg, mL, IU.
 *
 * Its own call, not part of the presentation vocabulary: offering "vial" beside
 * "mL" in one picker is how a strength would come to repeat what the
 * presentation already says.
 */
export async function listMeasurementUnits(): Promise<CodedValue[]> {
  const response = await apiFetch(buildUrl("/api/measurement-units"));

  if (!response.ok) {
    throw new Error("Unable to load measurement units.");
  }

  return response.json();
}
