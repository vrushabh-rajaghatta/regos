import { useQuery } from "@tanstack/react-query";

import { listComponents } from "../api/listComponents";

export function useComponents(medicinalProductId: string) {
  return useQuery({
    queryKey: ["components", medicinalProductId],
    queryFn: () => listComponents(medicinalProductId),
  });
}
