import { Badge } from "@/components/ui/badge";

// Maps domain status values to user-facing labels. Domain naming (e.g.
// "OnHold") stays separate from the terminology shown to users ("On Hold").
const STATUS_LABELS: Record<string, string> = {
  Draft: "Draft",
  Active: "Active",
  OnHold: "On Hold",
  Closed: "Closed",
};

interface ApplicationStatusBadgeProps {
  status: string;
}

export function ApplicationStatusBadge({ status }: ApplicationStatusBadgeProps) {
  return <Badge>{STATUS_LABELS[status] ?? status}</Badge>;
}
