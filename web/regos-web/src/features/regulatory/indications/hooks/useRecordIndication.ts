import { useMutation, useQueryClient } from "@tanstack/react-query";

import { recordIndication } from "../api/recordIndication";
import type { RecordIndicationBody } from "../api/recordIndication";

export function useRecordIndication(medicinalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: RecordIndicationBody) =>
      recordIndication(medicinalProductId, body),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["indications"] });
    },
  });
}
