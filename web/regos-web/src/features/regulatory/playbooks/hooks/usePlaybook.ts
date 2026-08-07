import { useQuery } from "@tanstack/react-query";

import { getPlaybook } from "../api/getPlaybook";

export function usePlaybook(id: string | undefined, version?: number) {
  return useQuery({
    queryKey: ["playbooks", id, version ?? "current"],
    queryFn: () => getPlaybook(id!, version),
    enabled: Boolean(id),
  });
}
