import { useMutation, useQueryClient } from "@tanstack/react-query";

import { stopCitingStudy } from "../api/stopCitingStudy";

export function useStopCitingStudy(applicationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (studyId: string) => stopCitingStudy(applicationId, studyId),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["applications", applicationId, "studies"],
      });
      queryClient.invalidateQueries({ queryKey: ["study-filings"] });
    },
  });
}
