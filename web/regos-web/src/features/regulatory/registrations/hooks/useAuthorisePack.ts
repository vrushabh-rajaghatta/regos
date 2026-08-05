import { useMutation, useQueryClient } from "@tanstack/react-query";

import { authorisePack } from "../api/authorisePack";

export function useAuthorisePack(medicinalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: {
      registrationId: string;
      packagedProductId: string;
      authorisedOn: string;
    }) =>
      authorisePack(input.registrationId, {
        packagedProductId: input.packagedProductId,
        authorisedOn: input.authorisedOn,
      }),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["packs", medicinalProductId],
      });
    },
  });
}
