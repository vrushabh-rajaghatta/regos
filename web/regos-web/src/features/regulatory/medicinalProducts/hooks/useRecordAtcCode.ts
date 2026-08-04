import { useMutation, useQueryClient } from "@tanstack/react-query";

import { recordAtcCode } from "../api/recordAtcCode";

export function useRecordAtcCode(medicinalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (atcCode: string | null) =>
      recordAtcCode(medicinalProductId, atcCode),

    // The ATC code is read from the market itself, so it is the market's query
    // that goes stale.
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["medicinal-products", "detail", medicinalProductId],
      });
    },
  });
}
