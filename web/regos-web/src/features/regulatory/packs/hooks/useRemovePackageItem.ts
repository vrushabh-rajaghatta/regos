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

      // A layer changes what the market-level read says about this pack (the
      // capstone shows a layer count), and that read is keyed under ["packs"].
      // Invalidated by prefix rather than by naming a market this hook does not
      // know — only the mounted market's query refetches.
      queryClient.invalidateQueries({ queryKey: ["packs"] });
    },
  });
}
