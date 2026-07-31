import { useState } from "react";
import { useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { PageHeader } from "@/shared/components/PageHeader";

import { CreateOrganizationSiteDialog } from "../components/CreateOrganizationSiteDialog";
import { OrganizationStatusBadge } from "../components/OrganizationStatusBadge";
import { SiteIdentifierList } from "../components/SiteIdentifierList";
import { useOrganizationSites } from "../hooks/useOrganizationSites";
import { siteTypeLabel } from "../types/OrganizationSiteType";

/**
 * Where this company operates.
 *
 * The mirror of the tenant-wide directory, and a separate server query rather
 * than the directory with an organization filter: these are two questions, and
 * this one implies the company that the directory has to name in every row.
 */
export function OrganizationSitesPage() {
  const { organizationId } = useParams();
  const [createOpen, setCreateOpen] = useState(false);

  const { data: sites, isPending, error } = useOrganizationSites(
    organizationId!,
  );

  return (
    <div className="p-6">
      <PageHeader
        title="Sites"
        description="Locations this organization operates"
        actions={<Button onClick={() => setCreateOpen(true)}>Add Site</Button>}
      />

      <CreateOrganizationSiteDialog
        organizationId={organizationId!}
        open={createOpen}
        onOpenChange={setCreateOpen}
      />

      <div className="mt-6">
        {isPending && <p data-testid="sites-loading">Loading sites...</p>}

        {error && <p data-testid="sites-error">Unable to load sites.</p>}

        {sites?.length === 0 && (
          <p className="text-muted-foreground" data-testid="sites-empty">
            No sites recorded.
          </p>
        )}

        {sites && sites.length > 0 && (
          <ul className="divide-y rounded-md border" data-testid="site-list">
            {sites.map((site) => (
              <li
                key={site.siteId}
                className="flex items-start justify-between gap-4 px-4 py-3"
                data-testid="site-row"
              >
                <div className="space-y-1">
                  <p className="font-medium">{site.name}</p>

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
        )}
      </div>
    </div>
  );
}
