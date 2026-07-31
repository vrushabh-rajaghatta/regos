import { useState } from "react";
import { Link, useParams } from "react-router-dom";

import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";

import { useCountries } from "../../masterData/hooks/useCountries";
import { RegistrationExpiry } from "../components/RegistrationExpiry";
import { RegistrationStatusBadge } from "../components/RegistrationStatusBadge";
import { RegistrationStatusFilter } from "../components/RegistrationStatusFilter";
import { useMarketRegistrations } from "../hooks/useMarketRegistrations";

/**
 * "What do we hold in this market?" — the mirror of the product portfolio.
 *
 * Every registration in the country appears, whatever its status: a withdrawn
 * authorisation is still part of the regulatory portfolio, and the server sends
 * live ones first. Narrowing the view is this page's business; hiding rows is
 * nobody's.
 */
export function MarketRegistrationsPage() {
  const { countryId } = useParams();
  const [status, setStatus] = useState("");

  const { data, isLoading, error } = useMarketRegistrations(countryId!);
  const countries = useCountries();

  const country = (countries.data ?? []).find((c) => c.id === countryId);

  const rows = (data ?? []).filter(
    (row) => status === "" || row.status === status
  );

  return (
    <Page>
      <PageHeader
        title={country?.name ?? "Market"}
        description="Every authorisation held in this market."
      />

      <Link
        to="/regulatory/registrations"
        className="text-sm text-primary hover:underline"
      >
        ← All markets
      </Link>

      {isLoading && (
        <p className="text-muted-foreground">Loading registrations...</p>
      )}

      {!isLoading && error && (
        <p className="text-destructive">Failed to load registrations.</p>
      )}

      {!isLoading && !error && data && data.length === 0 && (
        <div
          className="rounded-lg border border-dashed p-8 text-center"
          data-testid="market-registrations-empty"
        >
          <h3 className="text-lg font-semibold">
            Nothing is held in this market.
          </h3>
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
            <table className="w-full text-sm" data-testid="market-registrations">
              <thead className="bg-muted/50 text-left">
                <tr>
                  <th className="px-4 py-2 font-medium">Product</th>
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
                    data-testid="market-registration-row"
                  >
                    <td className="px-4 py-2">
                      <Link
                        to={`/regulatory/registrations/${row.registrationId}`}
                        className="font-medium text-primary hover:underline"
                      >
                        {row.productName}
                      </Link>

                      <span className="ml-2 text-xs text-muted-foreground">
                        {row.productCode}
                      </span>
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
    </Page>
  );
}
