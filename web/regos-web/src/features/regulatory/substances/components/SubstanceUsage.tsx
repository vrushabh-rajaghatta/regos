import { useState } from "react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Link } from "react-router-dom";

import { useProductsContainingSubstance } from "../hooks/useProductsContainingSubstance";
import type { SubstanceUsage as Usage } from "../types/SubstanceUsage";

interface SubstanceUsageProps {
  substanceId: string;
}

/**
 * Renders a strength the way a person reads one — the same two shapes the
 * composition editor shows, because they are the same fact seen from the other
 * end.
 */
function formatStrength(usage: Usage): string {
  const strength = usage.strength;

  if (!strength) return "no strength declared";

  const numerator = `${strength.numeratorValue} ${strength.numeratorUnit.display}`;

  return strength.denominatorValue === null || !strength.denominatorUnit
    ? numerator
    : `${numerator} / ${strength.denominatorValue} ${strength.denominatorUnit.display}`;
}

/**
 * *"Which of our products contain this substance?"* — the question EPIC-010a
 * exists to answer, on the row of the substance being asked about.
 *
 * **It is only askable because an ingredient stores an id.** Every hop from
 * here to a product is a join; a composition that carried substance names could
 * be read forwards only, and this would be a string match over free text.
 *
 * Asked rather than always shown, the same way a study's filings are: the
 * directory would otherwise run one join per row on every page load, and most
 * of the time nobody is asking.
 */
export function SubstanceUsage({ substanceId }: SubstanceUsageProps) {
  const [asked, setAsked] = useState(false);

  const { data, isLoading, error } = useProductsContainingSubstance(
    asked ? substanceId : null,
  );

  if (!asked) {
    return (
      <Button
        size="sm"
        variant="ghost"
        className="mt-1 px-0"
        data-testid="show-substance-usage"
        aria-label="Show the products that contain this substance"
        onClick={() => setAsked(true)}
      >
        Which products contain this?
      </Button>
    );
  }

  return (
    <div className="mt-2 text-sm" data-testid="substance-usage">
      {isLoading && <p className="text-muted-foreground">Loading products...</p>}

      {error && <p className="text-destructive">Failed to load products.</p>}

      {data?.length === 0 && (
        <p className="text-muted-foreground" data-testid="substance-usage-empty">
          No product contains this substance yet.
        </p>
      )}

      <ul className="space-y-1">
        {data?.map((usage) => (
          <li
            key={`${usage.presentationId}`}
            className="flex flex-wrap items-baseline gap-2"
            data-testid="substance-usage-row"
          >
            {/* Straight to the market, not the product: the reader asked an
                impact question, and the market is where the composition, the
                licences and the sale status all are. */}
            <Link
              to={`/regulatory/products/${usage.globalProductId}/markets/${usage.medicinalProductId}`}
              className="font-medium text-primary hover:underline"
            >
              {usage.productName}
            </Link>

            <span className="text-muted-foreground">{usage.countryName}</span>

            {/* An impact assessment is about what is on sale. A planned market
                and a launched one are very different phone calls. */}
            <Badge
              variant={usage.marketStatus === "Launched" ? "default" : "outline"}
            >
              {usage.marketStatus}
            </Badge>

            <span className="text-muted-foreground">
              {usage.presentationName}
            </span>

            <Badge variant={usage.role === "Active" ? "default" : "secondary"}>
              {usage.role}
            </Badge>

            <span className="text-muted-foreground">
              {formatStrength(usage)}
            </span>
          </li>
        ))}
      </ul>
    </div>
  );
}
