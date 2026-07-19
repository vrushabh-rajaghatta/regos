import { Badge } from "@/components/ui/badge";

// Maps domain status values to user-facing labels. The lifecycle is Draft then
// Published; more states (e.g. Archived) will join as the capability grows.
const STATUS_LABELS: Record<string, string> = {
  Draft: "Draft",
  Published: "Published",
};

interface SubmissionStatusBadgeProps {
  status: string;
}

export function SubmissionStatusBadge({ status }: SubmissionStatusBadgeProps) {
  return <Badge>{STATUS_LABELS[status] ?? status}</Badge>;
}
