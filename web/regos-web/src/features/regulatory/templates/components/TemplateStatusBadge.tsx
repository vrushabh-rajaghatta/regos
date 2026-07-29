import { Badge } from "@/components/ui/badge";

/**
 * Renders a template or version status. "Active" / "Published" read as the
 * settled, in-force state; "Draft" as work in progress; anything else
 * (e.g. "Deprecated") as retired.
 */
export function TemplateStatusBadge({ status }: { status: string }) {
  const variant =
    status === "Active" || status === "Published"
      ? "default"
      : status === "Draft"
        ? "outline"
        : "secondary";

  return (
    <Badge variant={variant} data-testid="template-status-badge">
      {status}
    </Badge>
  );
}
