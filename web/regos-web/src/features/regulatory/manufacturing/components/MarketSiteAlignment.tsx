import { Badge } from "@/components/ui/badge";

import { useSiteAlignment } from "../hooks/useSiteAlignment";

interface MarketSiteAlignmentProps {
  medicinalProductId: string;
}

/**
 * **"Where is this product made, and is that site on the licence?"** — the
 * question EPIC-010c was cut to answer, and the only place its two halves meet.
 *
 * **This panel introduces nothing.** It reads what S001 recorded and what S002
 * recorded and puts them side by side. Neither of those knows the other exists,
 * which is what makes a difference between them visible at all — an "approved
 * manufacturing operation" entity would have made this comparison impossible to
 * state.
 *
 * **Advisory, never blocking**, and the styling carries the decision: muted
 * prose, no destructive banner. A site manufacturing without approval is a real
 * regulatory finding and it is *not* this system's place to refuse the record
 * of it. The same call EPIC-005 made about an expired registration, EPIC-022
 * about a missing label language and an unaccepted stability condition — the
 * fourth time, and by now a house pattern.
 */
export function MarketSiteAlignment({
  medicinalProductId,
}: MarketSiteAlignmentProps) {
  const { data, isLoading, error } = useSiteAlignment(medicinalProductId);

  const rows = data ?? [];
  const diverging = rows.filter((row) => row.manufactures !== row.approved);

  return (
    <section className="space-y-3" data-testid="market-site-alignment">
      <div>
        <h2 className="text-lg font-semibold">
          Manufacturing against the licences
        </h2>
        <p className="text-sm text-muted-foreground">
          What happens, beside what this market's authorisations permit.
        </p>
      </div>

      {isLoading && (
        <p className="text-sm text-muted-foreground">Comparing...</p>
      )}
      {error && (
        <p className="text-sm text-destructive">
          Failed to compare sites against the licences.
        </p>
      )}

      {!isLoading && !error && rows.length === 0 && (
        <p
          className="rounded-lg border border-dashed p-6 text-center text-sm text-muted-foreground"
          data-testid="alignment-empty"
        >
          Nothing to compare yet. Record where the work happens and which sites
          the licences name, and the difference between them appears here.
        </p>
      )}

      {rows.length > 0 && (
        <table className="w-full text-sm" data-testid="alignment-table">
          <thead>
            <tr className="border-b text-left text-xs text-muted-foreground">
              <th className="py-2 pr-4 font-medium">Site</th>
              <th className="py-2 pr-4 font-medium">Performs</th>
              <th className="py-2 pr-4 font-medium">On a licence</th>
              <th className="py-2 font-medium">&nbsp;</th>
            </tr>
          </thead>

          <tbody>
            {rows.map((row) => (
              <tr
                key={row.organizationSiteId}
                className="border-b last:border-0 align-top"
                data-testid="alignment-row"
              >
                <td className="py-2 pr-4">
                  <span className="font-medium">{row.siteName}</span>
                  <span className="block text-xs text-muted-foreground">
                    {row.siteCountryName}
                  </span>
                </td>

                <td className="py-2 pr-4" data-testid="alignment-operations">
                  {row.operations.length > 0 ? (
                    row.operations.join(" · ")
                  ) : (
                    <span className="text-muted-foreground">—</span>
                  )}
                </td>

                <td className="py-2 pr-4" data-testid="alignment-approvals">
                  {row.approvals.length > 0 ? (
                    row.approvals
                      .map(
                        (approval) =>
                          `${approval.registrationNumber ?? "Number not issued"} (${approval.approvedOn})`,
                      )
                      .join(" · ")
                  ) : (
                    <span className="text-muted-foreground">—</span>
                  )}
                </td>

                <td className="py-2">
                  {/* Derived from the two facts beside it rather than sent as
                      a third — the rule is "both", and a stored verdict would
                      be a third thing to keep in sync. */}
                  {row.manufactures && row.approved && (
                    <Badge variant="secondary" data-testid="alignment-aligned">
                      Aligned
                    </Badge>
                  )}

                  {row.manufactures && !row.approved && (
                    <Badge
                      variant="outline"
                      data-testid="alignment-unapproved"
                    >
                      Not on a licence
                    </Badge>
                  )}

                  {!row.manufactures && row.approved && (
                    <Badge variant="outline" data-testid="alignment-unused">
                      Approved, not used
                    </Badge>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {/* Said once, below the table, in the muted voice this whole family of
          findings uses. Nothing above refuses anything, and this sentence is
          the only thing standing in for a rule. */}
      {diverging.length > 0 && (
        <p
          className="text-sm text-muted-foreground"
          data-testid="alignment-advisory"
        >
          {diverging.length}{" "}
          {diverging.length === 1 ? "site differs" : "sites differ"} from what
          the licences name. Reported, not prevented — a site may run before a
          variation lands, and a licence may name a site kept in reserve.
        </p>
      )}
    </section>
  );
}
