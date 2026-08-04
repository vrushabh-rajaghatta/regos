import { useQuery } from "@tanstack/react-query";

import { listProductsContainingSubstance } from "../api/listProductsContainingSubstance";

/**
 * Fetched only once asked. Most of the time nobody is asking, and the directory
 * would otherwise run one join per row on every page load — the same shape
 * `useStudyFilings` took for the same reason.
 */
export function useProductsContainingSubstance(substanceId: string | null) {
  return useQuery({
    queryKey: ["substance-usage", substanceId],
    queryFn: () => listProductsContainingSubstance(substanceId!),
    enabled: substanceId !== null,
  });
}
