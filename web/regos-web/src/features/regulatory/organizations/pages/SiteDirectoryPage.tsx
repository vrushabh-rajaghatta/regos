import { useState } from "react";
import { Link } from "react-router-dom";

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useCountries } from "@/features/regulatory/masterData/hooks/useCountries";
import { PageHeader } from "@/shared/components/PageHeader";

import { OrganizationStatusBadge } from "../components/OrganizationStatusBadge";
import { SiteIdentifierList } from "../components/SiteIdentifierList";
import { useSiteDirectory } from "../hooks/useSiteDirectory";
import {
  ORGANIZATION_SITE_TYPES,
  siteTypeLabel,
} from "../types/OrganizationSiteType";

const ANY = "any";

/**
 * "Which manufacturing sites do we have in India?" — across the whole registry
 * rather than within one company.
 *
 * This is the question that made OrganizationSite an aggregate root, so it sits
 * beside Organizations rather than beneath one, the same way Registrations sit
 * beside Products. Neither filter has a default: the unfiltered answer is a
 * legitimate question, not an accident.
 */
export function SiteDirectoryPage() {
  const [countryId, setCountryId] = useState(ANY);
  const [type, setType] = useState(ANY);

  const { data: countries } = useCountries();

  const { data: sites, isPending, error } = useSiteDirectory({
    countryId: countryId === ANY ? undefined : countryId,
    type: type === ANY ? undefined : type,
  });

  return (
    <div className="p-6">
      <PageHeader
        title="Sites"
        description="Every location in the registry, whoever operates it"
      />

      <div className="mt-6 flex flex-wrap gap-3">
        {/* The Select clears to null; "any" is this page's word for no filter,
            so the two are mapped rather than letting null reach the query. */}
        <Select
          value={countryId}
          onValueChange={(value) => setCountryId(value ?? ANY)}
        >
          <SelectTrigger className="w-56" data-testid="site-country-filter">
            <SelectValue />
          </SelectTrigger>

          <SelectContent>
            <SelectItem value={ANY}>All countries</SelectItem>

            {(countries ?? []).map((country) => (
              <SelectItem key={country.id} value={country.id}>
                {country.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select value={type} onValueChange={(value) => setType(value ?? ANY)}>
          <SelectTrigger className="w-56" data-testid="site-type-filter">
            <SelectValue />
          </SelectTrigger>

          <SelectContent>
            <SelectItem value={ANY}>All types</SelectItem>

            {ORGANIZATION_SITE_TYPES.map((option) => (
              <SelectItem key={option.value} value={option.value}>
                {option.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <div className="mt-6">
        {isPending && <p data-testid="site-directory-loading">Loading sites...</p>}

        {error && (
          <p data-testid="site-directory-error">Unable to load the directory.</p>
        )}

        {sites && (
          <>
            <p
              className="mb-3 text-sm text-muted-foreground"
              data-testid="site-directory-count"
            >
              {sites.length} {sites.length === 1 ? "site" : "sites"}
            </p>

            <ul
              className="divide-y rounded-md border"
              data-testid="site-directory"
            >
              {sites.map((site) => (
                <li
                  key={site.siteId}
                  className="flex items-start justify-between gap-4 px-4 py-3"
                  data-testid="site-directory-row"
                >
                  <div className="space-y-1">
                    <p className="font-medium">{site.name}</p>

                    {/* The organization is named, because the directory spans
                        the registry and the owner is not implied here. */}
                    <p className="text-sm">
                      <Link
                        to={`/regulatory/organizations/${site.organizationId}/sites`}
                        className="hover:underline"
                      >
                        {site.organizationName}
                      </Link>
                    </p>

                    <p className="text-sm text-muted-foreground">
                      {siteTypeLabel(site.type)} ·{" "}
                      {site.city ? `${site.city}, ` : ""}
                      {site.countryName}
                    </p>

                    <SiteIdentifierList identifiers={site.identifiers} />
                  </div>

                  <OrganizationStatusBadge status={site.status} />
                </li>
              ))}
            </ul>
          </>
        )}
      </div>
    </div>
  );
}
