import { keepPreviousData, useQuery } from "@tanstack/react-query";

import { listSubstances } from "../api/listSubstances";
import type { ListSubstancesParams } from "../api/listSubstances";

export function useSubstances(params: ListSubstancesParams = {}) {
  return useQuery({
    queryKey: ["substances", params.search ?? "", params.origin ?? "Any"],
    queryFn: () => listSubstances(params),

    // The list is a search result: keeping the previous page visible while the
    // next one loads stops the directory blanking on every keystroke.
    placeholderData: keepPreviousData,
  });
}
