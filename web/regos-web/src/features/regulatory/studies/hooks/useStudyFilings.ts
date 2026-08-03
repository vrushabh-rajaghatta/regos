import { useQuery } from "@tanstack/react-query";

import { listStudyFilings } from "../api/listStudyFilings";

/** Fetched only when a study is named, so the registry list stays one call. */
export function useStudyFilings(studyId: string | null) {
  return useQuery({
    queryKey: ["study-filings", studyId],
    queryFn: () => listStudyFilings(studyId!),
    enabled: studyId !== null,
  });
}
