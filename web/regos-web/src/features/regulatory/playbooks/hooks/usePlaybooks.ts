import { useQuery } from "@tanstack/react-query";

import { listPlaybooks } from "../api/listPlaybooks";

export function usePlaybooks() {
  return useQuery({
    queryKey: ["playbooks"],
    queryFn: listPlaybooks,
  });
}
