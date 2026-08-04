import { useQuery } from "@tanstack/react-query";

import { getClinicalVocabulary } from "../api/getClinicalVocabulary";

export function useClinicalVocabulary() {
  return useQuery({
    queryKey: ["indications", "vocabulary"],
    queryFn: getClinicalVocabulary,

    // Code, not data — it changes when the platform ships a new version.
    staleTime: Infinity,
  });
}
