import { useState } from "react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";

import { useIndications } from "../hooks/useIndications";
import { useRecordIndicationDecision } from "../hooks/useRecordIndicationDecision";
import { useRemoveIndicationPopulation } from "../hooks/useRemoveIndicationPopulation";
import type { Indication, Population } from "../types/Indication";
import { REGOS_INTERNAL } from "../types/Indication";

import { PopulationDialog } from "./PopulationDialog";
import { RecordIndicationDialog } from "./RecordIndicationDialog";

interface MarketIndicationsProps {
  medicinalProductId: string;
}

/** Renders an age band the way a label would say it. */
function ageOf(population: Population): string | null {
  if (population.ageLow === null && population.ageHigh === null) return null;

  const unit = population.ageUnitDisplay ?? "";

  if (population.ageHigh === null) return `${population.ageLow}+ ${unit}`;
  if (population.ageLow === null) return `up to ${population.ageHigh} ${unit}`;

  return `${population.ageLow}–${population.ageHigh} ${unit}`;
}

/**
 * What this product is approved to treat here.
 *
 * **Not what the label says about it** — that is a `LocalLabel` revision, on its
 * own clock. This is the authorisation, and its history is a sequence of
 * regulatory decisions rather than editions of a document (ADR-059).
 */
export function MarketIndications({
  medicinalProductId,
}: MarketIndicationsProps) {
  const [recording, setRecording] = useState(false);
  const [editing, setEditing] = useState<{
    indicationId: string;
    population?: Population;
  } | null>(null);

  const { data, isLoading, error } = useIndications(medicinalProductId);
  const decision = useRecordIndicationDecision();
  const removePopulation = useRemoveIndicationPopulation();

  const indications = data ?? [];

  return (
    <section className="space-y-3" data-testid="market-indications">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div>
          <h2 className="text-lg font-semibold">Indications</h2>
          <p className="text-sm text-muted-foreground">
            What this authority approved the product to treat, and who for.
          </p>
        </div>

        <Button
          size="sm"
          onClick={() => setRecording(true)}
          data-testid="record-indication"
        >
          Record indication
        </Button>
      </div>

      {isLoading && (
        <p className="text-sm text-muted-foreground">Loading indications...</p>
      )}
      {error && (
        <p className="text-sm text-destructive">Failed to load indications.</p>
      )}

      {/* A refused decision and a stale list look identical otherwise — SC-106. */}
      {decision.isError && (
        <p className="text-sm text-destructive" data-testid="decision-error">
          {(decision.error as Error).message}
        </p>
      )}

      {removePopulation.isError && (
        <p
          className="text-sm text-destructive"
          data-testid="remove-population-error"
        >
          {(removePopulation.error as Error).message}
        </p>
      )}

      {!isLoading && !error && indications.length === 0 && (
        <div
          className="rounded-lg border border-dashed p-6 text-center"
          data-testid="indications-empty"
        >
          <p className="text-sm text-muted-foreground">
            Nothing approved here yet.
          </p>
        </div>
      )}

      <ul className="space-y-3">
        {indications.map((indication: Indication) => (
          <li
            key={indication.id}
            className="rounded-lg border p-4"
            data-testid="indication-row"
          >
            <div className="flex flex-wrap items-baseline gap-3">
              <span className="font-semibold">
                {indication.conditionDisplay}
              </span>

              <Badge
                variant={
                  indication.currentStatus === "Withdrawn"
                    ? "outline"
                    : "default"
                }
                data-testid="indication-status"
              >
                {indication.currentStatus}
              </Badge>

              <span className="text-xs text-muted-foreground">
                since {indication.currentStatusOccurredOn}
              </span>

              {indication.conditionSystem === REGOS_INTERNAL && (
                <span className="text-xs text-muted-foreground">
                  RegOS terminology
                </span>
              )}
            </div>

            {/* The wording, kept visibly apart from the coded condition above:
                one is what the label says, the other is what makes the
                authorisation comparable across markets. */}
            <p className="mt-1 text-sm" data-testid="indication-text">
              {indication.labelText}
            </p>

            <div className="mt-2 space-y-1">
              {indication.populations.map((population) => (
                <div
                  key={population.id}
                  className="flex flex-wrap items-center gap-2 text-sm"
                  data-testid="population-row"
                >
                  <Badge variant="secondary">
                    {[
                      ageOf(population),
                      population.genderCode === "ALL"
                        ? null
                        : population.genderDisplay,
                      population.physiologicalConditionDisplay,
                    ]
                      .filter(Boolean)
                      .join(" · ") || "Everyone"}
                  </Badge>

                  {population.description && (
                    <span className="text-xs text-muted-foreground">
                      {population.description}
                    </span>
                  )}

                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-6 px-2 text-xs"
                    onClick={() =>
                      setEditing({ indicationId: indication.id, population })
                    }
                    data-testid="correct-population"
                  >
                    Correct
                  </Button>

                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-6 px-2 text-xs"
                    onClick={() =>
                      removePopulation.mutate({
                        indicationId: indication.id,
                        populationId: population.id,
                      })
                    }
                    data-testid="remove-population"
                  >
                    Remove
                  </Button>
                </div>
              ))}

              {indication.otherTherapies.map((therapy) => (
                <p
                  key={therapy.id}
                  className="text-xs text-muted-foreground"
                  data-testid="therapy-row"
                >
                  {therapy.relationshipDisplay} {therapy.therapy}
                </p>
              ))}
            </div>

            <div className="mt-2 flex flex-wrap items-center gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setEditing({ indicationId: indication.id })}
                data-testid="add-population"
              >
                Add population
              </Button>

              {indication.currentStatus !== "Withdrawn" && (
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() =>
                    decision.mutate({
                      indicationId: indication.id,
                      status: "Withdrawn",
                      occurredOn: new Date().toISOString().slice(0, 10),
                      note: null,
                    })
                  }
                  data-testid="withdraw-indication"
                >
                  Withdraw
                </Button>
              )}
            </div>

            {/* Every decision, never rewritten: it became withdrawn on a date. */}
            {indication.history.length > 1 && (
              <ul className="mt-2 border-t pt-2" data-testid="indication-history">
                {indication.history.map((entry) => (
                  <li
                    key={entry.id}
                    className="text-xs text-muted-foreground"
                    data-testid="decision-row"
                  >
                    {entry.status} — {entry.occurredOn}
                    {entry.note ? ` · ${entry.note}` : ""}
                  </li>
                ))}
              </ul>
            )}
          </li>
        ))}
      </ul>

      <RecordIndicationDialog
        medicinalProductId={medicinalProductId}
        open={recording}
        onOpenChange={setRecording}
      />

      {editing && (
        <PopulationDialog
          indicationId={editing.indicationId}
          population={editing.population}
          open
          onOpenChange={(next) => !next && setEditing(null)}
        />
      )}
    </section>
  );
}
