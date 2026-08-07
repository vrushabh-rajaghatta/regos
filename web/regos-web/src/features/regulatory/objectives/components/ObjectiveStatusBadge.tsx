import { Badge } from "@/components/ui/badge";

/**
 * An objective's status is about **intent**, not execution — a plan slipping
 * does not move it. "Active" is the settled working state; "Proposed" is stated
 * but not taken up; "Achieved" and "Abandoned" are terminal history.
 */
export function ObjectiveStatusBadge({ status }: { status: string }) {
  const variant =
    status === "Active"
      ? "default"
      : status === "Proposed"
        ? "outline"
        : "secondary";

  return (
    <Badge variant={variant} data-testid="objective-status-badge">
      {status}
    </Badge>
  );
}
