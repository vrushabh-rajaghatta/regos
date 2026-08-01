import { marketStatusLabel } from "../constants/marketStatuses";
import type { MedicinalProductDetail } from "../types/MedicinalProductDetail";

interface MarketOverviewProps {
  market: MedicinalProductDetail;
  registrationCount: number;
}

/**
 * Orientation before detail: the derived facts a reader wants before scrolling
 * into histories. Nothing here is editable — every value is either identity or
 * a projection over something recorded below.
 *
 * It is also where later epics accumulate. Strengths, packaging and label
 * status will each want a line here while their detail grows underneath, which
 * is the whole reason a market has a page rather than a row.
 */
export function MarketOverview({
  market,
  registrationCount,
}: MarketOverviewProps) {
  const facts: { label: string; value: string }[] = [
    { label: "On sale", value: marketStatusLabel(market.marketStatus) },
    // "Launched on", not "Launched": the field beside it can *hold* the value
    // "Launched", and a block reading "On sale: Launched / Launched: 2021-03-15"
    // makes a reader parse which is the label. The date wants a preposition.
    { label: "Launched on", value: market.launchedOn ?? "—" },
    { label: "Trade names", value: String(market.tradeNames.length) },
    { label: "Authorisations", value: String(registrationCount) },
    {
      label: "Record",
      value: market.status === "Active" ? "In use" : "Retired",
    },
  ];

  return (
    <dl
      className="grid grid-cols-2 gap-4 rounded-lg border p-4 sm:grid-cols-5"
      data-testid="market-overview"
    >
      {facts.map((fact) => (
        <div key={fact.label}>
          <dt className="text-xs text-muted-foreground">{fact.label}</dt>
          <dd className="mt-0.5 text-sm font-medium">{fact.value}</dd>
        </div>
      ))}
    </dl>
  );
}
