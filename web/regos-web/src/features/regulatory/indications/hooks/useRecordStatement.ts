import { useMutation, useQueryClient } from "@tanstack/react-query";

import { recordStatement } from "../api/recordStatement";
import type { RecordStatementBody } from "../api/recordStatement";
import type { StatementKind } from "../types/StatementKind";

export function useRecordStatement(
  kind: StatementKind,
  medicinalProductId: string,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (body: RecordStatementBody) =>
      recordStatement(kind, medicinalProductId, body),

    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: [kind] });
    },
  });
}
