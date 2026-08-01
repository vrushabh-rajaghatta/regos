import { useMutation, useQueryClient } from "@tanstack/react-query";

import { beginMeeting, type BeginMeetingBody } from "../api/beginMeeting";

export function useBeginMeeting() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: BeginMeetingBody) => beginMeeting(body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["meetings"] });
    },
  });
}
