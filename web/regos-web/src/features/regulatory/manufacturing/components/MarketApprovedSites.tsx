import { useState } from "react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";

import { useApproveSite } from "../hooks/useApproveSite";
import { useApprovedSites } from "../hooks/useApprovedSites";
import { useWithdrawSiteApproval } from "../hooks/useWithdrawSiteApproval";
import { useSiteDirectory } from "@/features/regulatory/organizations/hooks/useSiteDirectory";

interface MarketApprovedSitesProps {
  medicinalProductId: string;
  registrations: { id: string; registrationNumber: string | null }[];
}

/**
 * **"Which sites do this market's licences approve?"** — the other half of the
 * epic's question, and the half a regulator owns.
 *
 * **Grouped by site, not by licence.** A plant named on two of this market's
 * licences is one approved site with two dates, not two rows — because the
 * question asked of this panel is about the site.
 *
 * **This panel and the one above it are deliberately not joined.** What we do
 * and what the licence permits are separate statements from separate sources,
 * and comparing them is S004's job. Merging them here would make the divergence
 * impossible to see.
 */
export function MarketApprovedSites({
  medicinalProductId,
  registrations,
}: MarketApprovedSitesProps) {
  const { data, isLoading, error } = useApprovedSites(medicinalProductId);
  const { data: sites } = useSiteDirectory();

  const approve = useApproveSite(medicinalProductId);
  const withdraw = useWithdrawSiteApproval(medicinalProductId);

  const [approving, setApproving] = useState(false);

  const approved = data ?? [];

  return (
    <section className="space-y-3" data-testid="market-approved-sites">
      <div className="flex items-start justify-between gap-2">
        <div>
          <h2 className="text-lg font-semibold">
            What the licences approve
          </h2>
          <p className="text-sm text-muted-foreground">
            The manufacturing sites this market's authorisations name, and the
            date each was added.
          </p>
        </div>

        <Button
          size="sm"
          variant="outline"
          disabled={registrations.length === 0}
          onClick={() => setApproving((open) => !open)}
          data-testid="approve-site"
        >
          Add a site to a licence
        </Button>
      </div>

      {isLoading && (
        <p className="text-sm text-muted-foreground">Loading approvals...</p>
      )}
      {error && (
        <p className="text-sm text-destructive">
          Failed to load which sites are approved here.
        </p>
      )}

      {approve.isError && (
        <p className="text-sm text-destructive" data-testid="approve-site-error">
          {(approve.error as Error).message}
        </p>
      )}

      {withdraw.isError && (
        <p
          className="text-sm text-destructive"
          data-testid="withdraw-approval-error"
        >
          {(withdraw.error as Error).message}
        </p>
      )}

      {/* A market with no licence cannot approve anything, and saying so is
          better than an empty list that looks like a gap. */}
      {registrations.length === 0 && (
        <p
          className="rounded-lg border border-dashed p-6 text-center text-sm text-muted-foreground"
          data-testid="approved-sites-no-licence"
        >
          No licence yet. A market approves sites through its authorisations, so
          there is nothing here to record against.
        </p>
      )}

      {!isLoading &&
        !error &&
        registrations.length > 0 &&
        approved.length === 0 && (
          <p
            className="rounded-lg border border-dashed p-6 text-center text-sm text-muted-foreground"
            data-testid="approved-sites-empty"
          >
            No sites recorded against this market's licences yet.
          </p>
        )}

      {approving && (
        <ApproveRow
          registrations={registrations}
          sites={(sites ?? []).map((site) => ({
            id: site.siteId,
            label: `${site.name} — ${site.countryName}`,
          }))}
          onSubmit={(registrationId, organizationSiteId, approvedOn) =>
            approve.mutate(
              { registrationId, organizationSiteId, approvedOn },
              { onSuccess: () => setApproving(false) },
            )
          }
        />
      )}

      <ul className="space-y-2">
        {approved.map((site) => (
          <li
            key={site.organizationSiteId}
            className="rounded-lg border p-4"
            data-testid="approved-site-row"
          >
            <div className="flex flex-wrap items-baseline gap-2">
              <span className="font-medium">{site.siteName}</span>

              <span className="text-xs text-muted-foreground">
                {site.siteCountryName}
              </span>

              <Badge variant="secondary">
                {site.approvals.length}{" "}
                {site.approvals.length === 1 ? "licence" : "licences"}
              </Badge>
            </div>

            <ul className="mt-2 space-y-1">
              {site.approvals.map((approval) => (
                <li
                  key={approval.siteApprovalId}
                  className="flex flex-wrap items-center gap-2 text-sm"
                  data-testid="site-approval-row"
                >
                  <span className="font-mono text-xs">
                    {approval.registrationNumber ?? "Number not issued"}
                  </span>

                  <Badge variant="outline">{approval.registrationStatus}</Badge>

                  {/* The fact a foreign key could not carry, for the second
                      time in this codebase: a licence granted years earlier
                      may have gained this site by variation. */}
                  <span
                    className="text-xs text-muted-foreground"
                    data-testid="site-approved-on"
                  >
                    approved {approval.approvedOn}
                  </span>

                  <Button
                    size="sm"
                    variant="ghost"
                    onClick={() => withdraw.mutate(approval.siteApprovalId)}
                    data-testid="withdraw-site-approval"
                  >
                    Remove
                  </Button>
                </li>
              ))}
            </ul>
          </li>
        ))}
      </ul>
    </section>
  );
}

interface ApproveRowProps {
  registrations: { id: string; registrationNumber: string | null }[];
  sites: { id: string; label: string }[];
  onSubmit(
    registrationId: string,
    organizationSiteId: string,
    approvedOn: string,
  ): void;
}

/**
 * The date is asked for rather than assumed, for the reason a pack
 * authorisation's is: a site routinely joins a licence by variation, years
 * after it was granted.
 */
function ApproveRow({ registrations, sites, onSubmit }: ApproveRowProps) {
  const [registrationId, setRegistrationId] = useState("");
  const [siteId, setSiteId] = useState("");
  const [approvedOn, setApprovedOn] = useState("");

  return (
    <div className="flex flex-wrap items-end gap-2 rounded-md border border-dashed p-3">
      <div className="flex flex-col gap-1">
        <label htmlFor="approving-licence" className="text-xs">
          Licence
        </label>

        <select
          id="approving-licence"
          className="h-8 rounded-md border bg-transparent px-2 text-sm"
          value={registrationId}
          onChange={(event) => setRegistrationId(event.target.value)}
        >
          <option value="">Choose a licence</option>

          {registrations.map((registration) => (
            <option key={registration.id} value={registration.id}>
              {registration.registrationNumber ?? "Number not issued"}
            </option>
          ))}
        </select>
      </div>

      <div className="flex flex-col gap-1">
        <label htmlFor="approving-site" className="text-xs">
          Approved site
        </label>

        <select
          id="approving-site"
          className="h-8 rounded-md border bg-transparent px-2 text-sm"
          value={siteId}
          onChange={(event) => setSiteId(event.target.value)}
        >
          <option value="">Choose a site</option>

          {sites.map((site) => (
            <option key={site.id} value={site.id}>
              {site.label}
            </option>
          ))}
        </select>
      </div>

      <div className="flex flex-col gap-1">
        <label htmlFor="site-approved-on-input" className="text-xs">
          Added to the licence on
        </label>

        <input
          id="site-approved-on-input"
          type="date"
          className="h-8 rounded-md border bg-transparent px-2 text-sm"
          value={approvedOn}
          onChange={(event) => setApprovedOn(event.target.value)}
        />
      </div>

      <Button
        size="sm"
        disabled={registrationId === "" || siteId === "" || approvedOn === ""}
        onClick={() => onSubmit(registrationId, siteId, approvedOn)}
        data-testid="confirm-approve-site"
      >
        Record
      </Button>
    </div>
  );
}
