import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  addPackageItem,
  restatePackageItem,
  type PackageItemBody,
} from "../api/savePackageItem";

/**
 * One hook for add and restate: the caller has a layer or it does not, and
 * every consumer of both would otherwise branch twice.
 */
export function useSavePackageItem(packagedProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      packageItemId,
      body,
    }: {
      packageItemId?: string;
      body: PackageItemBody;
    }) =>
      packageItemId
        ? restatePackageItem(packageItemId, body)
        : addPackageItem(packagedProductId, body).then(() => undefined),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["package-items", packagedProductId],
      });
    },
  });
}
