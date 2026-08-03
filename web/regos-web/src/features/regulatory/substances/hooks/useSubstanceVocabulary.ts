import { useQuery } from "@tanstack/react-query";

import { getSubstanceVocabulary } from "../api/getSubstanceVocabulary";

export function useSubstanceVocabulary() {
  return useQuery({
    queryKey: ["substance-vocabulary"],
    queryFn: getSubstanceVocabulary,

    // It changes when the platform ships a new vocabulary, not while a form is
    // open.
    staleTime: Infinity,
  });
}
