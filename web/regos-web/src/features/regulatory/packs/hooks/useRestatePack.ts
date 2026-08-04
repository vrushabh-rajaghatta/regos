import { useMutation, useQueryClient } from "@tanstack/react-query";

import { restatePack, type RestatePackBody } from "../api/restatePack";

export function useRestatePack(medicinalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      packagedProductId,
      body,
    }: {
      packagedProductId: string;
      body: RestatePackBody;
    }) => restatePack(packagedProductId, body),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["packs", medicinalProductId],
      });
    },
  });
}
