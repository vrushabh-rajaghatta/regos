import { Link } from "react-router-dom";

import type { RegulatoryApplicationSummary } from "../types/RegulatoryApplicationSummary";
import { ApplicationStatusBadge } from "./ApplicationStatusBadge";

interface RegulatoryApplicationCardProps {
  globalProductId: string;
  application: RegulatoryApplicationSummary;
}

export function RegulatoryApplicationCard({
  globalProductId,
  application,
}: RegulatoryApplicationCardProps) {
  return (
    <Link
      to={`/regulatory/products/${globalProductId}/applications/${application.id}`}
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

        <ApplicationStatusBadge status={application.status} />
      </div>
    </Link>
  );
}
