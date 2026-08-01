import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  recordCorrespondence,
  type RecordCorrespondenceBody,
} from "../api/recordCorrespondence";

export function useRecordCorrespondence() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: RecordCorrespondenceBody) => recordCorrespondence(body),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["correspondence"] });
    },
  });
}
