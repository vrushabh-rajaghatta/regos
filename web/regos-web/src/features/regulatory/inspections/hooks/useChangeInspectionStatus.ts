import { useMutation, useQueryClient } from "@tanstack/react-query";

import { changeInspectionStatus } from "../api/changeInspectionStatus";

export function useChangeInspectionStatus(inspectionId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: { status: string; occurredOn: string }) =>
      changeInspectionStatus(inspectionId, input.status, input.occurredOn),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["inspections"] });
    },
  });
}
