import { useQuery } from "@tanstack/react-query";

import { listPackageItems } from "../api/listPackageItems";

export function usePackageItems(packagedProductId: string) {
  return useQuery({
    queryKey: ["package-items", packagedProductId],
    queryFn: () => listPackageItems(packagedProductId),
    enabled: !!packagedProductId,
  });
}
