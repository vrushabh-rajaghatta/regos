import { useQuery } from "@tanstack/react-query";

import { listMeetings } from "../api/listMeetings";

export function useMeetings(includeConcluded: boolean) {
  return useQuery({
    queryKey: ["meetings", includeConcluded],
    queryFn: () => listMeetings(includeConcluded),
  });
}
