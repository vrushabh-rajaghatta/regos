import { useQuery } from "@tanstack/react-query";

import { getLabelLanguageCoverage } from "../api/getLabelLanguageCoverage";

/**
 * Keyed under `["local-labels", …]` so that adding or removing a label
 * refreshes the advice by prefix, without every label mutation having to know
 * this panel exists — the call the capstone read made in EPIC-010b S005.
 */
export function useLabelLanguageCoverage(medicinalProductId: string) {
  return useQuery({
    queryKey: ["local-labels", medicinalProductId, "languages"],
    queryFn: () => getLabelLanguageCoverage(medicinalProductId),
    enabled: !!medicinalProductId,
  });
}
