import { useState } from "react";
import { Link } from "react-router-dom";

import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { REGOS_INTERNAL } from "@/shared/types/CodedConcept";

import { useClinicalVocabulary } from "../hooks/useClinicalVocabulary";
import { useMarketsForCondition } from "../hooks/useMarketsForCondition";
import type { ConditionMarket } from "../types/ConditionMarket";

/**
 * **"Which markets is this product approved for this condition in?"** — the
 * question EPIC-018 was built to answer.
 *
 * It works because the condition is **coded**, not because anything is stored
 * for reporting: France's indication and Canada's are separate aggregates with
 * separate wording, and the code is the only thing they share.
 *
 * **Every market that has an indication for the condition appears**, approved or
 * withdrawn. A silent filter would answer half the question and leave "and where
 * did we lose it?" to a second screen.
 *
 * Each row shows that market's own wording beside the shared code — ADR-059's
 * principle in one table. **Showing is not comparing**: side-by-side wording,
 * population diffs and divergence reports are EPIC-011.
 */
export function ProductIndicationMarkets({
  globalProductId,
}: {
  globalProductId: string;
}) {
  const [conditionCode, setConditionCode] = useState("");

  const vocabulary = useClinicalVocabulary();
  const { data, isLoading, error } = useMarketsForCondition(
    globalProductId,
    conditionCode,
  );

  const conditions = vocabulary.data?.conditions ?? [];
  const condition = conditions.find((c) => c.code === conditionCode);

  const markets = data ?? [];
  const approved = markets.filter((market) => market.isInForce);
  const withdrawn = markets.filter((market) => !market.isInForce);

  return (
    <section className="space-y-2" data-testid="indication-markets">
      <h2 className="text-sm font-medium text-muted-foreground">
        Where an indication is approved
      </h2>

      <div className="rounded-lg border p-4 space-y-4">
        <div className="flex flex-wrap items-center gap-3">
          <Select
            value={conditionCode}
            onValueChange={(code) => setConditionCode(code ?? "")}
          >
            <SelectTrigger className="w-80" data-testid="condition-picker">
              <SelectValue placeholder="Select a condition" />
            </SelectTrigger>

            <SelectContent>
              {conditions.map((concept) => (
                <SelectItem key={concept.code} value={concept.code}>
                  {concept.display}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>

          {/* Whose word this is, said out loud (ADR-058 §6). Eight terms,
              visibly a demonstration set rather than a clinical terminology. */}
          {condition?.system === REGOS_INTERNAL && (
            <span className="text-xs text-muted-foreground">
              RegOS terminology
            </span>
          )}
        </div>

        {!conditionCode && (
          <p className="text-sm text-muted-foreground" data-testid="no-condition">
            Choose a condition to see where this product is approved for it.
          </p>
        )}

        {conditionCode && isLoading && (
          <p className="text-muted-foreground">Loading markets...</p>
        )}

        {conditionCode && error && (
          <p className="text-destructive">Failed to load markets.</p>
        )}

        {/* Not an error, and not an empty screen: a coded question always has
            an answer, and "nowhere" is one of them. */}
        {conditionCode && !isLoading && !error && markets.length === 0 && (
          <p className="text-sm" data-testid="condition-nowhere">
            No market records this indication for this product.
          </p>
        )}

        {approved.length > 0 && (
          <ConditionMarkets
            globalProductId={globalProductId}
            heading={`Approved in ${approved.length} ${
              approved.length === 1 ? "market" : "markets"
            }`}
            markets={approved}
            testId="condition-approved"
          />
        )}

        {withdrawn.length > 0 && (
          <ConditionMarkets
            globalProductId={globalProductId}
            heading="No longer approved in"
            markets={withdrawn}
            testId="condition-withdrawn"
          />
        )}
      </div>
    </section>
  );
}

function ConditionMarkets({
  globalProductId,
  heading,
  markets,
  testId,
}: {
  globalProductId: string;
  heading: string;
  markets: ConditionMarket[];
  testId: string;
}) {
  return (
    <div className="space-y-2" data-testid={testId}>
      <h3 className="text-sm font-medium">{heading}</h3>

      <ul className="space-y-2">
        {markets.map((market) => (
          <li
            key={market.medicinalProductId}
            className="rounded-md border p-3"
            data-testid="condition-market-row"
          >
            <div className="flex flex-wrap items-baseline gap-3">
              <Link
                to={`/regulatory/products/${globalProductId}/markets/${market.medicinalProductId}`}
                className="font-medium text-primary hover:underline"
              >
                {market.countryName}
              </Link>

              <Badge
                variant={market.isInForce ? "default" : "outline"}
                data-testid="condition-market-status"
              >
                {market.status}
              </Badge>

              <span className="text-xs text-muted-foreground">
                since {market.since}
              </span>
            </div>

            {/* The same regulatory fact, in this market's own words. */}
            <p
              className="mt-1 text-sm text-muted-foreground"
              data-testid="condition-market-text"
            >
              {market.labelText}
            </p>
          </li>
        ))}
      </ul>
    </div>
  );
}
