import { useMutation, useQueryClient } from "@tanstack/react-query";

import { recordIndicationDecision } from "../api/recordIndicationDecision";
import type { RecordDecisionBody } from "../api/recordIndicationDecision";

interface DecisionInput extends RecordDecisionBody {
  indicationId: string;
}

export function useRecordIndicationDecision() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: DecisionInput) =>
      recordIndicationDecision(input.indicationId, {
        status: input.status,
        occurredOn: input.occurredOn,
        note: input.note,
      }),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["indications"] });
    },
  });
}
