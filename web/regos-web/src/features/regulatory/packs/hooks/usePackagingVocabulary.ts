import { useQuery } from "@tanstack/react-query";

import { getPackagingVocabulary } from "../api/getPackagingVocabulary";

export function usePackagingVocabulary() {
  return useQuery({
    queryKey: ["packaging", "vocabulary"],
    queryFn: getPackagingVocabulary,

    // Code, not data — it changes when the platform ships a new version.
    staleTime: Infinity,
  });
}
