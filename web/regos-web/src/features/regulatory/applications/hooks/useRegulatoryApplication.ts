import { useQuery } from "@tanstack/react-query";

import { getRegulatoryApplication } from "../api/getRegulatoryApplication";

export function useRegulatoryApplication(
  productId: string,
  applicationId: string
) {
  return useQuery({
    queryKey: ["products", productId, "applications", applicationId],
    queryFn: () => getRegulatoryApplication(productId, applicationId),
  });
}
