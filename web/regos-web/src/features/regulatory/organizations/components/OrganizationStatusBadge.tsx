import { Badge } from "@/components/ui/badge";

// Domain status values mapped to user-facing labels.
const STATUS_LABELS: Record<string, string> = {
  Active: "Active",
  Inactive: "Inactive",
};

const STATUS_VARIANTS: Record<string, "default" | "secondary" | "outline"> = {
  Active: "default",
  Inactive: "outline",
};

interface OrganizationStatusBadgeProps {
  status: string;
}

export function OrganizationStatusBadge({
  status,
}: OrganizationStatusBadgeProps) {
  return (
    <Badge variant={STATUS_VARIANTS[status] ?? "default"}>
      {STATUS_LABELS[status] ?? status}
    </Badge>
  );
}
