import { Badge } from "@/components/ui/badge";

import { statusLabel } from "./statusLabel";

/**
 * Which statuses read as live, which as pending, and which as ended. A visual
 * grouping only — the lifecycle itself lives on the server, and nothing here
 * decides what a registration may do next.
 */
const VARIANTS: Record<
  string,
  "default" | "secondary" | "outline" | "destructive"
> = {
  Approved: "default",
  Suspended: "destructive",
  Planned: "outline",
  Submitted: "secondary",
  UnderReview: "secondary",
  Expired: "outline",
  Withdrawn: "outline",
  Refused: "outline",
};

/**
 * Carries no test id of its own: the same badge renders in page headers, table
 * rows and every history entry, and a shared id would make "the status" mean
 * half a dozen things at once. Callers label the one that matters.
 */
export function RegistrationStatusBadge({ status }: { status: string }) {
  return (
    <Badge variant={VARIANTS[status] ?? "outline"}>{statusLabel(status)}</Badge>
  );
}
