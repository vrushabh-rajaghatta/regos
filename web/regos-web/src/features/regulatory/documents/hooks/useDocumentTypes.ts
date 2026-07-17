import { useQuery } from "@tanstack/react-query";

import { listDocumentTypes } from "../api/listDocumentTypes";

export function useDocumentTypes() {
  return useQuery({
    queryKey: ["reference-data", "document-types"],
    queryFn: listDocumentTypes,
  });
}
