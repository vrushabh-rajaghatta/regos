import { useState } from "react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { useAuthorisedPacks } from "../hooks/useAuthorisedPacks";
import { useAuthorisePack } from "../hooks/useAuthorisePack";
import { useWithdrawPackAuthorisation } from "../hooks/useWithdrawPackAuthorisation";
import type { AuthorisedPack } from "../types/AuthorisedPack";

interface MarketAuthorisedPacksProps {
  medicinalProductId: string;
  registrations: { id: string; registrationNumber: string | null }[];
}

/**
 * **"Which packs are authorised in this market, and how are they supplied?"**
 * — the question EPIC-010b was cut to answer.
 *
 * Every pack is listed, authorised or not. An unauthorised pack is not an error
 * and not a gap: a pack in design has no licence yet, and hiding it would make
 * the screen say the market sells less than it plans to.
 */
export function MarketAuthorisedPacks({
  medicinalProductId,
  registrations,
}: MarketAuthorisedPacksProps) {
  const { data, isLoading, error } = useAuthorisedPacks(medicinalProductId);
  const authorise = useAuthorisePack(medicinalProductId);
  const withdraw = useWithdrawPackAuthorisation(medicinalProductId);

  const [authorising, setAuthorising] = useState<AuthorisedPack | undefined>();

  const packs = data ?? [];

  return (
    <section className="space-y-3" data-testid="market-authorised-packs">
      <div>
        <h2 className="text-lg font-semibold">What this market may sell</h2>
        <p className="text-sm text-muted-foreground">
          Every pack, how it is supplied, and which licence authorises it.
        </p>
      </div>

      {isLoading && (
        <p className="text-sm text-muted-foreground">Loading packs...</p>
      )}
      {error && (
        <p className="text-sm text-destructive">
          Failed to load what this market may sell.
        </p>
      )}

      {authorise.isError && (
        <p className="text-sm text-destructive" data-testid="authorise-error">
          {(authorise.error as Error).message}
        </p>
      )}

      {withdraw.isError && (
        <p className="text-sm text-destructive" data-testid="withdraw-error">
          {(withdraw.error as Error).message}
        </p>
      )}

      {!isLoading && !error && packs.length === 0 && (
        <p
          className="rounded-lg border border-dashed p-6 text-center text-sm text-muted-foreground"
          data-testid="authorised-packs-empty"
        >
          No packs recorded yet. A market plans its packs before any of them is
          authorised.
        </p>
      )}

      <ul className="space-y-2">
        {packs.map((pack) => (
          <li
            key={pack.packagedProductId}
            className="rounded-lg border p-4"
            data-testid="authorised-pack-row"
          >
            <div className="flex flex-wrap items-baseline justify-between gap-2">
              <div className="flex flex-wrap items-baseline gap-2">
                <span className="font-medium">{pack.description}</span>

                {pack.packSizeQuantity !== null && (
                  <span className="text-xs text-muted-foreground">
                    {pack.packSizeQuantity} {pack.packSizeUnitDisplay}
                  </span>
                )}

                <Badge variant="secondary">{pack.currentMarketingStatus}</Badge>
              </div>

              {/* Authorised or not is the question this screen exists for, so
                  it is stated on every row rather than implied by absence. */}
              {pack.authorisations.length === 0 ? (
                <Badge variant="outline" data-testid="pack-unauthorised">
                  Not yet authorised
                </Badge>
              ) : (
                <Badge data-testid="pack-authorised">
                  Authorised under {pack.authorisations.length}{" "}
                  {pack.authorisations.length === 1 ? "licence" : "licences"}
                </Badge>
              )}
            </div>

            {/* How it is supplied — S003, read from the pack rather than
                restated here. */}
            <p
              className="mt-1 text-sm text-muted-foreground"
              data-testid="pack-supply-summary"
            >
              {[
                pack.legalStatusOfSupplyDisplay,
                pack.shelfLifeValue !== null
                  ? `${pack.shelfLifeValue} ${pack.shelfLifeUnitDisplay}`
                  : null,
                ...pack.storageConditions,
                pack.layerCount > 0
                  ? `${pack.layerCount} ${pack.layerCount === 1 ? "layer" : "layers"}`
                  : null,
              ]
                .filter(Boolean)
                .join(" · ") || "How it is supplied is not yet stated"}
            </p>

            {pack.authorisations.length > 0 && (
              <ul className="mt-2 space-y-1">
                {pack.authorisations.map((authorisation) => (
                  <li
                    key={authorisation.packAuthorisationId}
                    className="flex flex-wrap items-center gap-2 text-sm"
                    data-testid="pack-authorisation-row"
                  >
                    <span className="font-mono text-xs">
                      {authorisation.registrationNumber ?? "Number not issued"}
                    </span>

                    <Badge variant="outline">
                      {authorisation.registrationStatus}
                    </Badge>

                    {/* The fact a foreign key could not carry: a licence
                        granted years earlier may have gained this pack by
                        variation (ADR-061 §3). */}
                    <span
                      className="text-xs text-muted-foreground"
                      data-testid="authorised-on"
                    >
                      authorised {authorisation.authorisedOn}
                    </span>

                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={() =>
                        withdraw.mutate(authorisation.packAuthorisationId)
                      }
                      data-testid="withdraw-authorisation"
                    >
                      Remove
                    </Button>
                  </li>
                ))}
              </ul>
            )}

            <div className="mt-2">
              <Button
                size="sm"
                variant="outline"
                onClick={() =>
                  setAuthorising(
                    authorising?.packagedProductId === pack.packagedProductId
                      ? undefined
                      : pack,
                  )
                }
                data-testid="authorise-pack"
              >
                Authorise under a licence
              </Button>
            </div>

            {authorising?.packagedProductId === pack.packagedProductId && (
              <AuthoriseRow
                registrations={registrations}
                onSubmit={(registrationId, authorisedOn) => {
                  authorise.mutate(
                    {
                      registrationId,
                      packagedProductId: pack.packagedProductId,
                      authorisedOn,
                    },
                    { onSuccess: () => setAuthorising(undefined) },
                  );
                }}
              />
            )}
          </li>
        ))}
      </ul>
    </section>
  );
}

interface AuthoriseRowProps {
  registrations: { id: string; registrationNumber: string | null }[];
  onSubmit(registrationId: string, authorisedOn: string): void;
}

/**
 * The date is asked for rather than assumed, because it is routinely later than
 * the licence — a pack added by variation years afterwards.
 */
function AuthoriseRow({ registrations, onSubmit }: AuthoriseRowProps) {
  const [registrationId, setRegistrationId] = useState("");
  const [authorisedOn, setAuthorisedOn] = useState("");

  return (
    <div className="mt-2 flex flex-wrap items-end gap-2 rounded-md border border-dashed p-3">
      <div className="flex flex-col gap-1">
        <label htmlFor="authorising-licence" className="text-xs">
          Licence
        </label>

        <select
          id="authorising-licence"
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
        <label htmlFor="authorised-on-input" className="text-xs">
          Authorised on
        </label>

        <input
          id="authorised-on-input"
          type="date"
          className="h-8 rounded-md border bg-transparent px-2 text-sm"
          value={authorisedOn}
          onChange={(event) => setAuthorisedOn(event.target.value)}
        />
      </div>

      <Button
        size="sm"
        disabled={registrationId === "" || authorisedOn === ""}
        onClick={() => onSubmit(registrationId, authorisedOn)}
        data-testid="confirm-authorise-pack"
      >
        Authorise
      </Button>
    </div>
  );
}
