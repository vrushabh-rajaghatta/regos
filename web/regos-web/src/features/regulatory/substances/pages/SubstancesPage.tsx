import { useState } from "react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";

import { AddSubstanceDialog } from "../components/AddSubstanceDialog";
import { SubstanceUsage } from "../components/SubstanceUsage";
import { useSubstances } from "../hooks/useSubstances";
import {
  REGOS_INTERNAL,
  type SubstanceOrigin,
} from "../types/Substance";

const ORIGINS: { value: SubstanceOrigin; label: string }[] = [
  { value: "Any", label: "All" },
  { value: "Shared", label: "Shared catalogue" },
  { value: "Proprietary", label: "Ours" },
];

/**
 * The substance directory.
 *
 * Two catalogues in one list: the compounds the platform ships, and the ones
 * this organisation added. The distinction is shown rather than hidden, because
 * the two behave differently — a shared row is a governed fact nobody here can
 * change (ADR-058 §2).
 */
export function SubstancesPage() {
  const [adding, setAdding] = useState(false);
  const [search, setSearch] = useState("");
  const [origin, setOrigin] = useState<SubstanceOrigin>("Any");

  const { data, isLoading, error } = useSubstances({ search, origin });
  const rows = data ?? [];

  return (
    <Page>
      <PageHeader
        title="Substances"
        description="What products are made of. The shared catalogue, plus your own compounds."
        actions={
          <Button onClick={() => setAdding(true)} data-testid="add-substance">
            Add substance
          </Button>
        }
      />

      {/* RegOS ships six molecules and does not hold a licensed registry.
          Saying so on the screen is the same honesty ADR-058 §6 requires of
          the seed file — a user who expects a full pharmacopoeia should find
          out here, not by searching for a compound and concluding the search
          is broken. */}
      <p className="rounded-md border border-dashed p-3 text-xs text-muted-foreground">
        The shared catalogue is a small demonstration set. It is not the GSRS,
        UNII or ISO 11238 registry — add the compounds you work with, and they
        will sit alongside it.
      </p>

      <div className="flex flex-wrap items-center gap-2">
        <Input
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          placeholder="Search by name or INN"
          className="max-w-xs"
          data-testid="substance-search"
        />

        <div className="flex gap-1">
          {ORIGINS.map((option) => (
            <Button
              key={option.value}
              type="button"
              variant={origin === option.value ? "default" : "outline"}
              size="sm"
              onClick={() => setOrigin(option.value)}
              data-testid={`substance-origin-${option.value.toLowerCase()}`}
            >
              {option.label}
            </Button>
          ))}
        </div>
      </div>

      {isLoading && <p className="text-muted-foreground">Loading substances...</p>}
      {error && <p className="text-destructive">Failed to load substances.</p>}

      {!isLoading && !error && rows.length === 0 && (
        <div
          className="rounded-lg border border-dashed p-8 text-center"
          data-testid="substances-empty"
        >
          <h3 className="text-lg font-semibold">No substances match.</h3>
          <p className="mt-1 text-sm text-muted-foreground">
            Clear the search, or add the compound you were looking for.
          </p>
        </div>
      )}

      <ul className="space-y-3">
        {rows.map((substance) => (
          <li
            key={substance.id}
            className="rounded-lg border p-4"
            data-testid="substance-row"
          >
            <div className="flex flex-wrap items-baseline gap-3">
              <span className="font-semibold">{substance.name}</span>

              <Badge variant={substance.isShared ? "secondary" : "outline"}>
                {substance.isShared ? "Shared" : "Ours"}
              </Badge>

              {/* Whose word this is, said out loud. Every term RegOS ships
                  today is its own, and a screen that implied EDQM or GSRS
                  would be claiming terminology the platform does not hold. */}
              {substance.substanceClass.system === REGOS_INTERNAL && (
                <span className="text-xs text-muted-foreground">
                  RegOS terminology
                </span>
              )}
            </div>

            <p className="mt-1 text-sm text-muted-foreground">
              {substance.substanceClass.display} ·{" "}
              {substance.substanceType.display}
              {substance.inn && substance.inn !== substance.name && (
                <> · INN {substance.inn}</>
              )}
              {substance.molecularFormula && <> · {substance.molecularFormula}</>}
            </p>

            {substance.description && (
              <p className="mt-1 text-sm">{substance.description}</p>
            )}

            {/* The inverse question, on the substance's own row: "which
                products contain this?" — the question the whole epic exists to
                answer, and the only one that reads the composition backwards. */}
            <SubstanceUsage substanceId={substance.id} />

            {(substance.casNumber || substance.uniiCode) && (
              <p className="mt-1 font-mono text-xs text-muted-foreground">
                {substance.casNumber && <>CAS {substance.casNumber}</>}
                {substance.casNumber && substance.uniiCode && " · "}
                {substance.uniiCode && <>UNII {substance.uniiCode}</>}
              </p>
            )}
          </li>
        ))}
      </ul>

      <AddSubstanceDialog open={adding} onOpenChange={setAdding} />
    </Page>
  );
}
