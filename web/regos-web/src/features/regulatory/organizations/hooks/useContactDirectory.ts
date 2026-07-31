import { useQuery } from "@tanstack/react-query";

import { contactDirectory } from "../api/contactDirectory";

export function useContactDirectory(roleId?: string) {
  return useQuery({
    queryKey: ["contact-directory", roleId ?? null],
    queryFn: () => contactDirectory(roleId),
  });
}
