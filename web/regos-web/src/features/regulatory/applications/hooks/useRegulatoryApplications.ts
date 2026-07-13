import { useQuery } from "@tanstack/react-query";

import { listRegulatoryApplications } from "../api/listRegulatoryApplications";

export function useRegulatoryApplications(productId: string) {
  return useQuery({
    queryKey: ["products", productId, "applications"],
    queryFn: () => listRegulatoryApplications(productId),
  });
}
