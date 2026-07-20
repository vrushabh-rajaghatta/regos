import { Badge } from "@/components/ui/badge";

// Domain status values mapped to user-facing labels.
const STATUS_LABELS: Record<string, string> = {
  Invited: "Invited",
  Active: "Active",
  Inactive: "Inactive",
};

const STATUS_VARIANTS: Record<string, "default" | "secondary" | "outline"> = {
  Invited: "secondary",
  Active: "default",
  Inactive: "outline",
};

interface UserStatusBadgeProps {
  status: string;
}

export function UserStatusBadge({ status }: UserStatusBadgeProps) {
  return (
    <Badge variant={STATUS_VARIANTS[status] ?? "default"}>
      {STATUS_LABELS[status] ?? status}
    </Badge>
  );
}
