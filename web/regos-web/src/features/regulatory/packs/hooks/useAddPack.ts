import { useMutation, useQueryClient } from "@tanstack/react-query";

import { addPack, type PackBody } from "../api/addPack";

export function useAddPack(medicinalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: PackBody) => addPack(medicinalProductId, body),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["packs", medicinalProductId],
      });
    },
  });
}
