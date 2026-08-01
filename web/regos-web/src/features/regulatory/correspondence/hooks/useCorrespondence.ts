import { useQuery } from "@tanstack/react-query";

import { getCorrespondence } from "../api/getCorrespondence";

export function useCorrespondence(correspondenceId: string) {
  return useQuery({
    queryKey: ["correspondence", "detail", correspondenceId],
    queryFn: () => getCorrespondence(correspondenceId),
    enabled: correspondenceId !== "",
  });
}
