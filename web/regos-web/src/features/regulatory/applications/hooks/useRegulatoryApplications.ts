import { useQuery } from "@tanstack/react-query";

import { listRegulatoryApplications } from "../api/listRegulatoryApplications";

export function useRegulatoryApplications(globalProductId: string) {
  return useQuery({
    queryKey: ["products", globalProductId, "applications"],
    queryFn: () => listRegulatoryApplications(globalProductId),
  });
}
