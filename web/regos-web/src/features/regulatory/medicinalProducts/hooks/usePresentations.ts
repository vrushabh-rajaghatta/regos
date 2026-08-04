import { useQuery } from "@tanstack/react-query";

import { listPresentations } from "../api/listPresentations";

export function usePresentations(medicinalProductId: string) {
  return useQuery({
    queryKey: ["presentations", medicinalProductId],
    queryFn: () => listPresentations(medicinalProductId),
  });
}
