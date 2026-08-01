import { useState } from "react";
import { Link, useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";

import { CreateRegistrationDialog } from "../../registrations/components/CreateRegistrationDialog";
import { RegistrationExpiry } from "../../registrations/components/RegistrationExpiry";
import { RegistrationStatusBadge } from "../../registrations/components/RegistrationStatusBadge";
import { useProductRegistrations } from "../../registrations/hooks/useProductRegistrations";
import { AddTradeNameDialog } from "../components/AddTradeNameDialog";
import { ChangeMarketStatusDialog } from "../components/ChangeMarketStatusDialog";
import { MarketActivationDialog } from "../components/MarketActivationDialog";
import { MarketOverview } from "../components/MarketOverview";
import { MarketStatusTimeline } from "../components/MarketStatusTimeline";
import { MarketTradeNames } from "../components/MarketTradeNames";
import { useMedicinalProduct } from "../hooks/useMedicinalProduct";

/**
 * One market, as the working surface it has become.
 *
 * Until EPIC-017 S004 a market was a row, and every capability the tier gained
 * had to be squeezed into it as another button. It is not a row any more: it
 * carries trade names, a commercial history, an operability flag and the
 * licences granted over it — and labels, packaging and strengths are all
 * waiting for somewhere to attach. **The route is the expensive decision;
 * whether this later grows tabs is cheap.**
 *
 * Sections, not tabs: three short sections read as one narrative top to bottom,
 * and `OrganizationWorkspaceLayout` earned its tabs by having four genuinely
 * separate directories. This does not yet.
 */
export function MedicinalProductPage() {
  const { globalProductId, medicinalProductId } = useParams();

  const [naming, setNaming] = useState(false);
  const [restatusing, setRestatusing] = useState(false);
  const [retiring, setRetiring] = useState(false);
  const [registering, setRegistering] = useState(false);

  const { data: market, isLoading, error } = useMedicinalProduct(
    medicinalProductId!
  );

  // The registrations come from the Registration slice's own query, filtered to
  // this market — the Product context does not read them (ADR-039 principle 5).
  const registrations = useProductRegistrations(globalProductId!);

  const held = (registrations.data ?? []).filter(
    (row) => row.medicinalProductId === medicinalProductId
  );

  if (isLoading) {
    return (
      <Page>
        <p className="text-muted-foreground">Loading market...</p>
      </Page>
    );
  }

  if (error) {
    return (
      <Page>
        <p className="text-destructive">Failed to load the market.</p>
      </Page>
    );
  }

  if (!market) {
    return (
      <Page>
        <div
          className="rounded-lg border border-dashed p-8 text-center"
          data-testid="market-not-found"
        >
          <h3 className="text-lg font-semibold">This market does not exist.</h3>
          <p className="mt-1 text-sm text-muted-foreground">
            It may have been removed, or the link may be wrong.
          </p>
        </div>
      </Page>
    );
  }

  return (
    <Page>
      <PageHeader
        title={market.countryName}
        description={`${market.productName} — ${market.productCode}`}
        actions={
          <div className="flex gap-2">
            <Button variant="outline" onClick={() => setRetiring(true)}>
              {market.status === "Inactive" ? "Restore" : "Retire"}
            </Button>

            <Button onClick={() => setRegistering(true)}>
              New registration
            </Button>
          </div>
        }
      />

      <MarketOverview market={market} registrationCount={held.length} />

      <section className="space-y-2">
        <div className="flex items-center justify-between">
          <h2 className="text-sm font-medium text-muted-foreground">
            Trade names
          </h2>

          <Button variant="ghost" size="sm" onClick={() => setNaming(true)}>
            Add name
          </Button>
        </div>

        <MarketTradeNames
          medicinalProductId={market.medicinalProductId}
          tradeNames={market.tradeNames}
        />
      </section>

      <section className="space-y-2">
        <div className="flex items-center justify-between">
          <h2 className="text-sm font-medium text-muted-foreground">
            Commercial history
          </h2>

          <Button
            variant="ghost"
            size="sm"
            onClick={() => setRestatusing(true)}
          >
            Record sale status
          </Button>
        </div>

        <MarketStatusTimeline history={market.marketStatusHistory} />
      </section>

      <section className="space-y-2">
        <h2 className="text-sm font-medium text-muted-foreground">
          Authorisations held here
        </h2>

        {held.length === 0 && (
          <p className="text-sm text-muted-foreground" data-testid="market-unauthorised">
            Nothing authorised here yet. A market exists from the moment you
            intend to sell in it.
          </p>
        )}

        {held.length > 0 && (
          <div className="overflow-x-auto rounded-lg border">
            <table className="w-full text-sm" data-testid="market-registrations">
              <thead className="bg-muted/50 text-left">
                <tr>
                  <th className="px-4 py-2 font-medium">Authority</th>
                  <th className="px-4 py-2 font-medium">Number</th>
                  <th className="px-4 py-2 font-medium">Holder</th>
                  <th className="px-4 py-2 font-medium">Status</th>
                  <th className="px-4 py-2 font-medium">Expires</th>
                </tr>
              </thead>

              <tbody>
                {held.map((row) => (
                  <tr
                    key={row.registrationId}
                    className="border-t"
                    data-testid="market-registration"
                  >
                    <td className="px-4 py-2">
                      <Link
                        to={`/regulatory/registrations/${row.registrationId}`}
                        className="font-medium text-primary hover:underline"
                      >
                        {row.authorityName}
                      </Link>
                    </td>
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
        )}
      </section>

      <AddTradeNameDialog
        medicinalProductId={market.medicinalProductId}
        countryName={market.countryName}
        open={naming}
        onOpenChange={setNaming}
      />

      <ChangeMarketStatusDialog
        medicinalProductId={market.medicinalProductId}
        countryName={market.countryName}
        open={restatusing}
        onOpenChange={setRestatusing}
      />

      <MarketActivationDialog
        medicinalProductId={market.medicinalProductId}
        countryName={market.countryName}
        active={market.status === "Inactive"}
        registrationCount={held.length}
        open={retiring}
        onOpenChange={setRetiring}
      />

      {registering && (
        <CreateRegistrationDialog
          market={market}
          open
          onOpenChange={(open) => {
            if (!open) setRegistering(false);
          }}
        />
      )}
    </Page>
  );
}
