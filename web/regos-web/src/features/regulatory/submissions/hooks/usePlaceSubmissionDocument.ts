import { useMutation, useQueryClient } from "@tanstack/react-query";

import { placeSubmissionDocument } from "../api/placeSubmissionDocument";

export interface PlaceSubmissionDocumentVariables {
  submissionDocumentId: string;
  /** Null takes the document out of the structure without detaching it. */
  templateSectionId: string | null;
}

export function usePlaceSubmissionDocument(submissionId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      submissionDocumentId,
      templateSectionId,
    }: PlaceSubmissionDocumentVariables) =>
      placeSubmissionDocument(
        submissionId,
        submissionDocumentId,
        templateSectionId
      ),

    onSuccess: () => {
      // Placement is what completeness is derived from, so both read models
      // this workspace composes change together.
      queryClient.invalidateQueries({
        queryKey: ["submissions", submissionId, "content-plan"],
      });
      queryClient.invalidateQueries({
        queryKey: ["submissions", submissionId, "validation"],
      });
    },
  });
}
