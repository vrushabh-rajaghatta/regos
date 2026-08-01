import { useMutation, useQueryClient } from "@tanstack/react-query";

import { attachCorrespondenceContent } from "../api/attachCorrespondenceContent";

export function useAttachCorrespondenceContent(correspondenceId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (file: File) =>
      attachCorrespondenceContent(correspondenceId, file),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["correspondence"] });
    },
  });
}
