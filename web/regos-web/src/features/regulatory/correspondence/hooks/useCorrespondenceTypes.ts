import { useQuery } from "@tanstack/react-query";

import { listCorrespondenceTypes } from "../api/listCorrespondenceTypes";

export function useCorrespondenceTypes() {
  return useQuery({
    queryKey: ["correspondence", "types"],
    queryFn: listCorrespondenceTypes,
    staleTime: 5 * 60 * 1000,
  });
}
