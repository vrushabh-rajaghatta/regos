import { Badge } from "@/components/ui/badge";

/**
 * Renders a playbook or version status. "Active" / "Published" read as the
 * settled, in-force state; "Draft" as work in progress; "Superseded" and
 * "Retired" as history that is still readable.
 */
export function PlaybookStatusBadge({ status }: { status: string }) {
  const variant =
    status === "Active" || status === "Published"
      ? "default"
      : status === "Draft"
        ? "outline"
        : "secondary";

  return (
    <Badge variant={variant} data-testid="playbook-status-badge">
      {status}
    </Badge>
  );
}
