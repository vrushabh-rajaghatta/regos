import { useState } from "react";
import { Link, useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";

import { AddMarketDialog } from "../../medicinalProducts/components/AddMarketDialog";
import { MarketTradeNames } from "../../medicinalProducts/components/MarketTradeNames";
import { marketStatusLabel } from "../../medicinalProducts/constants/marketStatuses";
import { useMedicinalProducts } from "../../medicinalProducts/hooks/useMedicinalProducts";
import type { MedicinalProduct } from "../../medicinalProducts/types/MedicinalProduct";
import { CreateRegistrationDialog } from "../components/CreateRegistrationDialog";
import { RegistrationExpiry } from "../components/RegistrationExpiry";
import { RegistrationStatusBadge } from "../components/RegistrationStatusBadge";
import { RegistrationStatusFilter } from "../components/RegistrationStatusFilter";
import { useProductRegistrations } from "../hooks/useProductRegistrations";

/**
 * "Where is this product registered?" — one half of the portfolio question,
 * now answered in two layers.
 *
 * The markets come first because they are what a licence is granted over: a
 * product is present in a market from the moment the company intends to sell
 * there, which is years before an authority agrees. A market with no
 * registrations is an ordinary state, not an empty table.
 *
 * Every registration row links to the registration's own page rather than to a
 * copy of it nested here: a registration is an aggregate in its own right, and
 * it has one canonical URL whichever direction you arrive from.
 */
export function ProductRegistrationsPage() {
  const { globalProductId } = useParams();
  const [addingMarket, setAddingMarket] = useState(false);
  const [registeringIn, setRegisteringIn] = useState<MedicinalProduct | null>(
    null
  );
  const [status, setStatus] = useState("");

  const markets = useMedicinalProducts(globalProductId!);
  const { data, isLoading, error } = useProductRegistrations(globalProductId!);

  const rows = (data ?? []).filter(
    (row) => status === "" || row.status === status
  );

  return (
    <Page>
      <PageHeader
        title="Registrations"
        description="The markets this product is present in, and the authorisations it holds in them."
        actions={
          <Button onClick={() => setAddingMarket(true)}>Add market</Button>
        }
      />

      <section className="space-y-2">
        <h2 className="text-sm font-medium text-muted-foreground">Markets</h2>

        {markets.isLoading && (
          <p className="text-muted-foreground">Loading markets...</p>
        )}

        {!markets.isLoading && markets.error && (
          <p className="text-destructive">Failed to load markets.</p>
        )}

        {!markets.isLoading && !markets.error && markets.data?.length === 0 && (
          <div
            className="rounded-lg border border-dashed p-8 text-center"
            data-testid="product-markets-empty"
          >
            <h3 className="text-lg font-semibold">
              This product is not in any market yet.
            </h3>
            <p className="mt-1 text-sm text-muted-foreground">
              Add the market first — an authorisation is granted over a product
              in a country, not over the product itself.
            </p>
          </div>
        )}

        {!markets.isLoading && !markets.error && !!markets.data?.length && (
          <div className="overflow-x-auto rounded-lg border">
            <table className="w-full text-sm" data-testid="product-markets">
              <thead className="bg-muted/50 text-left">
                <tr>
                  <th className="px-4 py-2 font-medium">Market</th>
                  <th className="px-4 py-2 font-medium">Called</th>
                  <th className="px-4 py-2 font-medium">On sale</th>
                  <th className="px-4 py-2 font-medium">Launched on</th>
                  <th className="px-4 py-2 font-medium">Authorisations</th>
                  <th className="px-4 py-2" />
                </tr>
              </thead>

              <tbody>
                {markets.data.map((market) => (
                  <tr
                    key={market.medicinalProductId}
                    className="border-t"
                    data-testid="product-market-row"
                  >
                    <td className="px-4 py-2 font-medium">
                      <Link
                        to={`/regulatory/products/${globalProductId}/markets/${market.medicinalProductId}`}
                        className="text-primary hover:underline"
                      >
                        {market.countryName}
                      </Link>

                      {/* Retired markets stay visible, labelled. Hiding them
                          would be data loss dressed as a default. */}
                      {market.status === "Inactive" && (
                        <span
                          className="ml-2 rounded bg-muted px-1.5 py-0.5 text-xs font-normal text-muted-foreground"
                          data-testid="market-retired"
                        >
                          Retired
                        </span>
                      )}
                    </td>
                    <td className="px-4 py-2">
                      <MarketTradeNames
                        medicinalProductId={market.medicinalProductId}
                        tradeNames={market.tradeNames}
                      />
                    </td>
                    <td className="px-4 py-2" data-testid="market-status">
                      {marketStatusLabel(market.marketStatus)}
                    </td>
                    <td className="px-4 py-2" data-testid="market-launched">
                      {market.launchedOn ?? (
                        <span className="text-muted-foreground">—</span>
                      )}
                    </td>
                    <td className="px-4 py-2">
                      {
                        (data ?? []).filter(
                          (row) =>
                            row.medicinalProductId === market.medicinalProductId
                        ).length
                      }
                    </td>
                    {/* One action, not five. Naming, sale status and
                        retirement moved to the market's own page in S004 —
                        the row is a summary again, and the next capability
                        this tier gains has somewhere to go that is not here. */}
                    <td className="px-4 py-2 text-right whitespace-nowrap">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => setRegisteringIn(market)}
                      >
                        New registration
                      </Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      {isLoading && (
        <p className="text-muted-foreground">Loading registrations...</p>
      )}

      {!isLoading && error && (
        <p className="text-destructive">Failed to load registrations.</p>
      )}

      {!isLoading && !error && data && data.length === 0 && (
        <div
          className="rounded-lg border border-dashed p-8 text-center"
          data-testid="product-registrations-empty"
        >
          <h3 className="text-lg font-semibold">
            This product is not registered anywhere yet.
          </h3>
          <p className="mt-1 text-sm text-muted-foreground">
            Record an authorisation to start tracking where it may be marketed.
          </p>
        </div>
      )}

      {!isLoading && !error && data && data.length > 0 && (
        <div className="space-y-4">
          <RegistrationStatusFilter
            statuses={data.map((row) => row.status)}
            value={status}
            onChange={setStatus}
          />

          <div className="overflow-x-auto rounded-lg border">
            <table className="w-full text-sm" data-testid="product-registrations">
              <thead className="bg-muted/50 text-left">
                <tr>
                  <th className="px-4 py-2 font-medium">Market</th>
                  <th className="px-4 py-2 font-medium">Authority</th>
                  <th className="px-4 py-2 font-medium">Number</th>
                  <th className="px-4 py-2 font-medium">Holder</th>
                  <th className="px-4 py-2 font-medium">Status</th>
                  <th className="px-4 py-2 font-medium">Expires</th>
                </tr>
              </thead>

              <tbody>
                {rows.map((row) => (
                  <tr
                    key={row.registrationId}
                    className="border-t"
                    data-testid="product-registration-row"
                  >
                    <td className="px-4 py-2">
                      <Link
                        to={`/regulatory/registrations/${row.registrationId}`}
                        className="font-medium text-primary hover:underline"
                      >
                        {row.countryName}
                      </Link>
                    </td>
                    <td className="px-4 py-2">{row.authorityName}</td>
                    <td className="px-4 py-2">
                      {row.registrationNumber ?? (
                        <span className="text-muted-foreground">—</span>
                      )}
                    </td>
                    <td className="px-4 py-2">{row.holderOrganizationName}</td>
                    <td className="px-4 py-2">
                      <RegistrationStatusBadge status={row.status} />
                    </td>
                    <td className="px-4 py-2">
                      <RegistrationExpiry
                        expiresOn={row.expiresOn}
                        daysUntilExpiry={row.daysUntilExpiry}
                        hasRunningValidity={row.hasRunningValidity}
                        isExpired={row.isExpired}
                      />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      <AddMarketDialog
        globalProductId={globalProductId!}
        open={addingMarket}
        onOpenChange={setAddingMarket}
      />

                        {/* Mounted per market so the dialog never has to ask which one it is in;
          unmounting on close is also what resets the form. */}
      {registeringIn && (
        <CreateRegistrationDialog
          market={registeringIn}
          open
          onOpenChange={(open) => {
            if (!open) setRegisteringIn(null);
          }}
        />
      )}
    </Page>
  );
}
