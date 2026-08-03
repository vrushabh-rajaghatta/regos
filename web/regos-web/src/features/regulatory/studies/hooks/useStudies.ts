import { useQuery } from "@tanstack/react-query";

import { listStudies } from "../api/listStudies";

export function useStudies() {
  return useQuery({
    queryKey: ["studies"],
    queryFn: listStudies,
  });
}
