import { useMutation, useQueryClient } from "@tanstack/react-query";

import { addIndicationTherapy } from "../api/addIndicationTherapy";

interface TherapyInput {
  indicationId: string;
  relationshipCode: string;
  therapy: string;
}

export function useAddIndicationTherapy() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: TherapyInput) =>
      addIndicationTherapy(input.indicationId, {
        relationshipCode: input.relationshipCode,
        therapy: input.therapy,
      }),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["indications"] });
    },
  });
}
