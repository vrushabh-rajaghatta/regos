import { useMutation, useQueryClient } from "@tanstack/react-query";

import { recordRegistrationApproval } from "../api/recordRegistrationApproval";
import type { RecordApprovalBody } from "../api/recordRegistrationApproval";

export function useRecordRegistrationApproval(registrationId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: RecordApprovalBody) =>
      recordRegistrationApproval(registrationId, body),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["registrations"] });
    },
  });
}
