import { Badge } from "@/components/ui/badge";

// Maps domain status values to user-facing labels. Submissions only have a
// single status today (Draft); the map keeps room for the lifecycle we will
// introduce in a future sprint, mirroring ApplicationStatusBadge.
const STATUS_LABELS: Record<string, string> = {
  Draft: "Draft",
};

interface SubmissionStatusBadgeProps {
  status: string;
}

export function SubmissionStatusBadge({ status }: SubmissionStatusBadgeProps) {
  return <Badge>{STATUS_LABELS[status] ?? status}</Badge>;
}
