import { useMutation, useQueryClient } from "@tanstack/react-query";

import { beginInspection, type BeginInspectionBody } from "../api/beginInspection";

export function useBeginInspection() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: BeginInspectionBody) => beginInspection(body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["inspections"] });
    },
  });
}
