import { useMutation, useQueryClient } from "@tanstack/react-query";

import { citeStudy } from "../api/citeStudy";
import type { StudyKind } from "../../studies";

export function useCiteStudy(applicationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (study: { id: string; kind: StudyKind }) =>
      citeStudy(applicationId, study),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["applications", applicationId, "studies"],
      });

      // The inverse view changes too: this study now has one more filing.
      queryClient.invalidateQueries({ queryKey: ["study-filings"] });
    },
  });
}
