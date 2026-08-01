import { useMutation, useQueryClient } from "@tanstack/react-query";

import { changeMeetingStatus } from "../api/changeMeetingStatus";

export function useChangeMeetingStatus(meetingId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: { status: string; occurredOn: string }) =>
      changeMeetingStatus(meetingId, input.status, input.occurredOn),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["meetings"] });
    },
  });
}
