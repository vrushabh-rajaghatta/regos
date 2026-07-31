import { Link } from "react-router-dom";

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

      {!isLoading && !error && data && data.length > 0 && (
        <ul className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3" data-testid="registration-markets">
          {data.map((market) => (
            <li key={market.countryId}>
              <Link
                to={`/regulatory/registrations/markets/${market.countryId}`}
                className="flex items-center justify-between rounded-lg border p-4 hover:bg-muted"
                data-testid="registration-market"
              >
                <span className="font-medium">{market.countryName}</span>

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
