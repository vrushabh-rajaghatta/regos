import { useQuery } from "@tanstack/react-query";

import { listObjectives } from "../api/listObjectives";

export function useObjectives(includeClosed = false) {
  return useQuery({
    queryKey: ["objectives", { includeClosed }],
    queryFn: () => listObjectives(includeClosed),
  });
}
