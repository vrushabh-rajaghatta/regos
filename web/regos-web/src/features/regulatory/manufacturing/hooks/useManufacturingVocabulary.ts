import { useQuery } from "@tanstack/react-query";

import { getManufacturingVocabulary } from "../api/getManufacturingVocabulary";

export function useManufacturingVocabulary() {
  return useQuery({
    queryKey: ["manufacturing", "vocabulary"],
    queryFn: getManufacturingVocabulary,

    // Code, not data — it changes when the platform ships a new version.
    staleTime: Infinity,
  });
}
