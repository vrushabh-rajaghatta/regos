import { useMutation } from "@tanstack/react-query";

import { generateSequencePackage } from "../api/generateSequencePackage";

/**
 * No cache invalidation, and that absence is ADR-049: generating a package
 * changes nothing about the submission. It is a projection of a frozen
 * sequence, so there is no server state to refetch afterwards.
 */
export function useGenerateSequencePackage(submissionId: string) {
  return useMutation({
    mutationFn: () => generateSequencePackage(submissionId),

    onSuccess: ({ fileName, blob }) => {
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");

      link.href = url;
      link.download = fileName;
      link.click();

      URL.revokeObjectURL(url);
    },
  });
}
