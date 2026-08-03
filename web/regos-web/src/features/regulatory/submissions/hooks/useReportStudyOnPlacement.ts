import { useMutation, useQueryClient } from "@tanstack/react-query";

import { reportStudyOnPlacement } from "../api/reportStudyOnPlacement";
import type { StudyKind } from "../../studies";

export interface ReportStudyVariables {
  submissionDocumentId: string;
  /** Null says this placement reports no study after all. */
  study: { id: string; kind: StudyKind } | null;
  /** ICH's `file-tag`. Null when the placement plays no named role. */
  fileTag: string | null;
}

export function useReportStudyOnPlacement(submissionId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      submissionDocumentId,
      study,
      fileTag,
    }: ReportStudyVariables) =>
      reportStudyOnPlacement(
        submissionId,
        submissionDocumentId,
        study,
        fileTag,
      ),

    onSuccess: () => {
      // The content plan carries the reported study on each placement, so it
      // is the read that changes. Validation does not: whether a study is
      // owed is answered at generation, not by the blueprint.
      queryClient.invalidateQueries({
        queryKey: ["submissions", submissionId, "content-plan"],
      });
    },
  });
}
