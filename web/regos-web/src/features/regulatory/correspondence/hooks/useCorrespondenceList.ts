import { useQuery } from "@tanstack/react-query";

import {
  listCorrespondence,
  type ListCorrespondenceFilters,
} from "../api/listCorrespondence";

export function useCorrespondenceList(
  filters: ListCorrespondenceFilters = {},
) {
  return useQuery({
    queryKey: ["correspondence", "list", filters],
    queryFn: () => listCorrespondence(filters),
  });
}
