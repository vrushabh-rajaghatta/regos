import { useQuery } from "@tanstack/react-query";

import { getPharmaceuticalVocabulary } from "../api/getPharmaceuticalVocabulary";

export function usePharmaceuticalVocabulary() {
  return useQuery({
    queryKey: ["pharmaceutical-vocabulary"],
    queryFn: getPharmaceuticalVocabulary,

    // It changes when the platform ships a new vocabulary, not while a form is
    // open.
    staleTime: Infinity,
  });
}
