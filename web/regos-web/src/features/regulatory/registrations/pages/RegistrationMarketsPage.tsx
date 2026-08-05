import { useState } from "react";
import { Link } from "react-router-dom";

import { Badge } from "@/components/ui/badge";
import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";

import { ExpiringRegistrations } from "../components/ExpiringRegistrations";
import { useRegistrationMarkets } from "../hooks/useRegistrationMarkets";

/**
 * The way into the market view: the countries something is actually held in.
 *
 * Deliberately thin — a list, not a dashboard. Nobody starts by browsing two
 * hundred countries to find the three they are in, and the count only says
 * whether a market is worth opening. Breakdowns and trends are EPIC-011.
 */
export function RegistrationMarketsPage() {
  const { data, isLoading, error } = useRegistrationMarkets();

  // "Which of our markets are in the EU?" — the question EPIC-022 S002 exists
  // to make answerable. Filtered here rather than served as its own endpoint:
  // the portfolio is small enough that the list already travels, and the
  // groupings overlap, so a market can match more than one.
  const [region, setRegion] = useState("");

  const markets = (data ?? []).filter(
    (market) => region === "" || market.regions.includes(region),
  );

  const regionsInUse = [
    ...new Set((data ?? []).flatMap((market) => market.regions)),
  ].sort();

  return (
    <Page>
      <PageHeader
        title="Registrations"
        description="The markets this organisation holds authorisations in."
      />

      {/*
        The portfolio's front door answers two questions: what needs looking at,
        and where we hold things. Attention comes first because it is the one
        with a deadline.
      */}
      <ExpiringRegistrations />

      {regionsInUse.length > 0 && (
        <div className="flex flex-wrap items-center gap-2">
          <label htmlFor="region-filter" className="text-sm">
            Grouping
          </label>

          <select
            id="region-filter"
            className="h-8 rounded-md border bg-transparent px-2 text-sm"
            value={region}
            onChange={(event) => setRegion(event.target.value)}
            data-testid="region-filter"
          >
            <option value="">All markets</option>

            {regionsInUse.map((code) => (
              <option key={code} value={code}>
                {code === "PIC_S" ? "PIC/S" : code}
              </option>
            ))}
          </select>
        </div>
      )}

      {isLoading && <p className="text-muted-foreground">Loading markets...</p>}

      {!isLoading && error && (
        <p className="text-destructive">Failed to load the markets.</p>
      )}

      {!isLoading && !error && data && data.length === 0 && (
        <div
          className="rounded-lg border border-dashed p-8 text-center"
          data-testid="registration-markets-empty"
        >
          <h3 className="text-lg font-semibold">
            Nothing is registered anywhere yet.
          </h3>
          <p className="mt-1 text-sm text-muted-foreground">
            Markets appear here once a product holds an authorisation in one.
          </p>
        </div>
      )}

      {/*
        A filter that matches nothing is not the same as a portfolio that holds
        nothing, and the empty state below says the second — so this says the
        first out loud rather than letting them look identical.
      */}
      {!isLoading && !error && data && data.length > 0 && markets.length === 0 && (
        <p
          className="rounded-lg border border-dashed p-6 text-center text-sm text-muted-foreground"
          data-testid="no-markets-in-region"
        >
          No markets in this grouping. India, for one, belongs to none of them.
        </p>
      )}

      {!isLoading && !error && data && data.length > 0 && (
        <ul className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3" data-testid="registration-markets">
          {markets.map((market) => (
            <li key={market.countryId}>
              <Link
                to={`/regulatory/registrations/markets/${market.countryId}`}
                className="flex items-center justify-between rounded-lg border p-4 hover:bg-muted"
                data-testid="registration-market"
              >
                <span className="flex flex-wrap items-center gap-2">
                  <span className="font-medium">{market.countryName}</span>

                  {market.regions.map((code) => (
                    <Badge key={code} variant="secondary" className="text-xs">
                      {code === "PIC_S" ? "PIC/S" : code}
                    </Badge>
                  ))}
                </span>

                <span className="text-sm text-muted-foreground">
                  {market.registrationCount}
                </span>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </Page>
  );
}
