import { Link } from "react-router-dom";

import { expiryPhrase, needsAttention } from "./expiry";
import { RegistrationStatusBadge } from "./RegistrationStatusBadge";
import { useExpiringRegistrations } from "../hooks/useRegistrations";

/**
 * "Which registrations deserve attention today?"
 *
 * The server sends every authorisation still on the validity timeline, nearest
 * expiry first, and takes no view on which of them matter. This decides what to
 * put in front of someone — the only place that threshold exists — and offers
 * the rest behind a count rather than hiding them.
 */
export function ExpiringRegistrations() {
  const { data, isLoading, error } = useExpiringRegistrations();

  if (isLoading || error || !data) return null;

  const attention = data.filter((row) => needsAttention(row.daysUntilExpiry));

  if (attention.length === 0) return null;

  return (
    <section className="space-y-3" data-testid="expiring-registrations">
      <h2 className="text-lg font-medium">Needs attention</h2>

      <ul className="divide-y rounded-lg border">
        {attention.map((row) => (
          <li key={row.registrationId} className="p-4">
            <Link
              to={`/regulatory/registrations/${row.registrationId}`}
              className="flex flex-wrap items-center justify-between gap-2"
              data-testid="expiring-registration"
            >
              <span>
                <span className="font-medium text-primary hover:underline">
                  {row.productName}
                </span>
                <span className="ml-2 text-sm text-muted-foreground">
                  {row.countryName}
                  {row.registrationNumber && ` · ${row.registrationNumber}`}
                </span>
              </span>

              <span className="flex items-center gap-3">
                <RegistrationStatusBadge status={row.status} />

                <span
                  className={
                    row.isExpired
                      ? "text-sm font-medium text-destructive"
                      : "text-sm font-medium"
                  }
                >
                  {expiryPhrase(row.daysUntilExpiry)}
                </span>
              </span>
            </Link>
          </li>
        ))}
      </ul>

      {data.length > attention.length && (
        <p className="text-sm text-muted-foreground">
          {data.length - attention.length} further authorisation
          {data.length - attention.length === 1 ? "" : "s"} expire later.
        </p>
      )}
    </section>
  );
}
