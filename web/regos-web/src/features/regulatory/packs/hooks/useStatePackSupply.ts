import { useMutation, useQueryClient } from "@tanstack/react-query";

import { statePackSupply, type PackSupplyBody } from "../api/statePackSupply";

export function useStatePackSupply(medicinalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      packagedProductId,
      body,
    }: {
      packagedProductId: string;
      body: PackSupplyBody;
    }) => statePackSupply(packagedProductId, body),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["packs", medicinalProductId],
      });
    },
  });
}
