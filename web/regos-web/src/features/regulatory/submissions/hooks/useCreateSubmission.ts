import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  createSubmission,
  type CreateSubmissionRequest,
} from "../api/createSubmission";

export function useCreateSubmission(applicationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateSubmissionRequest) =>
      createSubmission(applicationId, request),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["applications", applicationId, "submissions"],
      });
    },
  });
}
