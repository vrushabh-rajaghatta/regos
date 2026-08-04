import { useMutation, useQueryClient } from "@tanstack/react-query";

import { recordInteraction } from "../api/recordInteraction";
import type { RecordInteractionBody } from "../api/recordInteraction";

export function useRecordInteraction(medicinalProductId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: RecordInteractionBody) =>
      recordInteraction(medicinalProductId, body),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["interactions"] });
    },
  });
}
