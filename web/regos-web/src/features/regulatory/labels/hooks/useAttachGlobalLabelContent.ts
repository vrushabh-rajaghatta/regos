import { useMutation, useQueryClient } from "@tanstack/react-query";

import { attachGlobalLabelContent } from "../api/attachGlobalLabelContent";

interface AttachInput {
  globalLabelId: string;
  versionId: string;
  contentId: string;
}

export function useAttachGlobalLabelContent() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: AttachInput) =>
      attachGlobalLabelContent(
        input.globalLabelId,
        input.versionId,
        input.contentId,
      ),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["global-labels"] });
    },
  });
}
