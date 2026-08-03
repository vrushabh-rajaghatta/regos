import { useMutation, useQueryClient } from "@tanstack/react-query";

import { recordApplicationNumber } from "../api/recordApplicationNumber";

export function useRecordApplicationNumber(applicationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (applicationNumber: string) =>
      recordApplicationNumber(applicationId, applicationNumber),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["applications", applicationId],
      });
    },
  });
}
