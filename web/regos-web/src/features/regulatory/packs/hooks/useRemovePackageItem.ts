import { useMutation, useQueryClient } from "@tanstack/react-query";

import { removePackageItem } from "../api/removePackageItem";

export function useRemovePackageItem(packagedProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (packageItemId: string) => removePackageItem(packageItemId),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["package-items", packagedProductId],
      });
    },
  });
}
