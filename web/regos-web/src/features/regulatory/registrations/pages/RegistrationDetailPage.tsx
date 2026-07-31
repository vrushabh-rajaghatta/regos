import { useState } from "react";
import { Link, useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";
import { PageSection } from "@/shared/components/PageSection";

import { ChangeRegistrationStatusDialog } from "../components/ChangeRegistrationStatusDialog";
import { RegistrationExpiry } from "../components/RegistrationExpiry";
import { RegistrationHistoryTimeline } from "../components/RegistrationHistoryTimeline";
import { RegistrationStatusBadge } from "../components/RegistrationStatusBadge";
import { statusLabel } from "../components/statusLabel";
import { useRegistration } from "../hooks/useRegistrations";

/**
 * A registration's own page — one canonical URL, reached from either portfolio
 * axis, because a registration is an aggregate rather than a view of a product.
 *
 * The actions are whatever the server said were possible. This page never asks
 * "may I offer Suspend?", only "did `allowedNextStatuses` include it?" — so the
 * lifecycle rules exist in exactly one place, and a terminal registration
 * simply arrives with nothing to offer.
 */
export function RegistrationDetailPage() {
  const { registrationId } = useParams();
  const [target, setTarget] = useState<string | null>(null);

  const { data, isLoading, error } = useRegistration(registrationId!);

  return (
    <Page>
      {isLoading && (
        <p className="text-muted-foreground">Loading the registration...</p>
      )}

      {!isLoading && error && (
        <p className="text-destructive">Failed to load the registration.</p>
      )}

      {!isLoading && !error && data && (
        <>
          <PageHeader
            title={data.registrationNumber ?? "Not yet granted"}
            description={`${data.productName} — ${data.countryName}`}
            actions={
              <span data-testid="registration-status">
                <RegistrationStatusBadge status={data.status} />
              </span>
            }
          />

          <div className="flex flex-wrap gap-4 text-sm">
            <Link
              to={`/regulatory/products/${data.productId}/registrations`}
              className="text-primary hover:underline"
            >
              {data.productName}
            </Link>

            <Link
              to={`/regulatory/registrations/markets/${data.countryId}`}
              className="text-primary hover:underline"
            >
              {data.countryName}
            </Link>
          </div>

          <PageSection title="Authorisation">
            <dl
              className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4"
              data-testid="registration-facts"
            >
              <Fact label="Authority" value={data.authorityName} />
              <Fact label="Holder" value={data.holderOrganizationName} />
              <Fact
                label="Approved"
                value={
                  data.approvedOn
                    ? new Date(data.approvedOn).toLocaleDateString()
                    : "—"
                }
              />
              <div>
                <dt className="text-xs text-muted-foreground">Expires</dt>
                <dd className="mt-0.5 font-medium">
                  <RegistrationExpiry
                    expiresOn={data.expiresOn}
                    daysUntilExpiry={data.daysUntilExpiry}
                    hasRunningValidity={data.hasRunningValidity}
                    isExpired={data.isExpired}
                  />
                </dd>
              </div>
            </dl>
          </PageSection>

          <PageSection title="Lifecycle">
            {data.allowedNextStatuses.length === 0 ? (
              <p
                className="text-sm text-muted-foreground"
                data-testid="registration-terminal"
              >
                This registration has reached the end of its lifecycle.
              </p>
            ) : (
              <div
                className="flex flex-wrap gap-2"
                data-testid="registration-actions"
              >
                {data.allowedNextStatuses.map((status) => (
                  <Button
                    key={status}
                    variant="outline"
                    onClick={() => setTarget(status)}
                  >
                    {/*
                      Becoming Approved with no number yet is the grant; the
                      dialog reads the record to tell which it is.
                    */}
                    {status === "Approved" && data.registrationNumber === null
                      ? "Record grant"
                      : statusLabel(status)}
                  </Button>
                ))}
              </div>
            )}
          </PageSection>

          <PageSection title="History">
            <RegistrationHistoryTimeline history={data.history} />
          </PageSection>

          {target !== null && (
            <ChangeRegistrationStatusDialog
              registration={data}
              target={target}
              onClose={() => setTarget(null)}
            />
          )}
        </>
      )}
    </Page>
  );
}

function Fact({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <dt className="text-xs text-muted-foreground">{label}</dt>
      <dd className="mt-0.5 font-medium">{value}</dd>
    </div>
  );
}
