import { useState } from "react";
import { Link, useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";

import { CreateRegistrationDialog } from "../components/CreateRegistrationDialog";
import { RegistrationExpiry } from "../components/RegistrationExpiry";
import { RegistrationStatusBadge } from "../components/RegistrationStatusBadge";
import { RegistrationStatusFilter } from "../components/RegistrationStatusFilter";
import { useProductRegistrations } from "../hooks/useProductRegistrations";

/**
 * "Where is this product registered?" — one half of the portfolio question.
 *
 * Every row links to the registration's own page rather than to a copy of it
 * nested under this product: a registration is an aggregate in its own right,
 * and it has one canonical URL whichever direction you arrive from.
 */
export function ProductRegistrationsPage() {
  const { globalProductId } = useParams();
  const [creating, setCreating] = useState(false);
  const [status, setStatus] = useState("");

  const { data, isLoading, error } = useProductRegistrations(globalProductId!);

  const rows = (data ?? []).filter(
    (row) => status === "" || row.status === status
  );

  return (
    <Page>
      <PageHeader
        title="Registrations"
        description="The market authorisations this product holds."
        actions={
          <Button onClick={() => setCreating(true)}>New registration</Button>
        }
      />

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

      <CreateRegistrationDialog
        globalProductId={globalProductId!}
        open={creating}
        onOpenChange={setCreating}
      />
    </Page>
  );
}
