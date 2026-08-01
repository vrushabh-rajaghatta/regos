import { useMutation, useQueryClient } from "@tanstack/react-query";

import { removeCorrespondenceContent } from "../api/removeCorrespondenceContent";

export function useRemoveCorrespondenceContent(correspondenceId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (attachmentId: string) =>
      removeCorrespondenceContent(correspondenceId, attachmentId),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["correspondence"] });
    },
  });
}
