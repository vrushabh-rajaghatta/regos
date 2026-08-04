import { useQuery } from "@tanstack/react-query";

import { getSupplyVocabulary } from "../api/getSupplyVocabulary";

export function useSupplyVocabulary() {
  return useQuery({
    queryKey: ["supply", "vocabulary"],
    queryFn: getSupplyVocabulary,

    // Code, not data — it changes when the platform ships a new version.
    staleTime: Infinity,
  });
}
