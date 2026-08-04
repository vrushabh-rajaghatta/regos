import { useQuery } from "@tanstack/react-query";

import { getLabelVocabulary } from "../api/getLabelVocabulary";

export function useLabelVocabulary() {
  return useQuery({
    queryKey: ["labels", "vocabulary"],
    queryFn: getLabelVocabulary,

    // Code, not data — it changes when the platform ships a new version, never
    // between two reads in one session.
    staleTime: Infinity,
  });
}
