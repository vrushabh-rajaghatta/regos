import { Link } from "react-router-dom";

import { Badge } from "@/components/ui/badge";

import type { RegulatoryApplicationSummary } from "../types/RegulatoryApplicationSummary";

interface RegulatoryApplicationCardProps {
  productId: string;
  application: RegulatoryApplicationSummary;
}

export function RegulatoryApplicationCard({
  productId,
  application,
}: RegulatoryApplicationCardProps) {
  return (
    <Link
      to={`/regulatory/products/${productId}/applications/${application.id}`}
      className="block rounded-lg border p-4 transition-colors hover:bg-muted/50"
    >
      <div className="flex items-start justify-between gap-4">
        <div className="space-y-1">
          <h3 className="font-semibold">{application.name}</h3>

          {/* Regulatory teams think in jurisdictions: Country (Authority). */}
          <p className="text-sm text-muted-foreground">
            {application.countryName} ({application.authorityCode})
          </p>

          <p className="text-sm text-muted-foreground">
            {application.organizationName}
          </p>
        </div>

        <Badge>{application.status}</Badge>
      </div>
    </Link>
  );
}
