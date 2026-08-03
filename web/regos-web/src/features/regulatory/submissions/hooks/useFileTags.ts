import { useQuery } from "@tanstack/react-query";

import { listFileTags } from "../api/listFileTags";

export function useFileTags() {
  return useQuery({
    queryKey: ["file-tags"],
    queryFn: listFileTags,
    // A published vocabulary; it changes when ICH republishes, not per session.
    staleTime: Infinity,
  });
}
