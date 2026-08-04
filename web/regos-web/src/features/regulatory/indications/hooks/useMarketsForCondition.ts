import { useQuery } from "@tanstack/react-query";

import { listMarketsForCondition } from "../api/listMarketsForCondition";

/**
 * Nothing is fetched until a condition is chosen — an unasked question has no
 * answer, and an empty list would read as "approved nowhere".
 */
export function useMarketsForCondition(
  globalProductId: string,
  conditionCode: string,
) {
  return useQuery({
    queryKey: ["indications", "markets", globalProductId, conditionCode],
    queryFn: () => listMarketsForCondition(globalProductId, conditionCode),
    enabled: !!globalProductId && !!conditionCode,
  });
}
