import { Badge } from "@/components/ui/badge";

const STATUS_LABELS: Record<string, string> = {
  Draft: "Draft",
  Active: "Active",
  Archived: "Archived",
};

interface ProductDocumentStatusBadgeProps {
  status: string;
}

export function ProductDocumentStatusBadge({
  status,
}: ProductDocumentStatusBadgeProps) {
  return <Badge>{STATUS_LABELS[status] ?? status}</Badge>;
}
