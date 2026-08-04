import { useMutation, useQueryClient } from "@tanstack/react-query";

import { movePackageItem } from "../api/movePackageItem";

export function useMovePackageItem(packagedProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      packageItemId,
      newParentPackageItemId,
    }: {
      packageItemId: string;
      newParentPackageItemId: string | null;
    }) => movePackageItem(packageItemId, newParentPackageItemId),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["package-items", packagedProductId],
      });
    },
  });
}
