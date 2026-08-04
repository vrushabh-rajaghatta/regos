import { useState } from "react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";

import { useInteractions } from "../hooks/useInteractions";
import { useRemoveStatementPopulation } from "../hooks/useRemoveStatementPopulation";
import type { Population } from "../types/Indication";

import { PopulationDialog } from "./PopulationDialog";
import { RecordInteractionDialog } from "./RecordInteractionDialog";

interface MarketInteractionsProps {
  medicinalProductId: string;
}

function describe(population: Population): string {
  const age =
    population.ageLow === null && population.ageHigh === null
      ? null
      : population.ageHigh === null
        ? `${population.ageLow}+ ${population.ageUnitDisplay ?? ""}`
        : population.ageLow === null
          ? `up to ${population.ageHigh} ${population.ageUnitDisplay ?? ""}`
          : `${population.ageLow}–${population.ageHigh} ${population.ageUnitDisplay ?? ""}`;

  return (
    [
      age,
      population.genderCode === "ALL" ? null : population.genderDisplay,
      population.physiologicalConditionDisplay,
    ]
      .filter(Boolean)
      .join(" · ") || "Everyone"
  );
}

/**
 * What this product clashes with here.
 *
 * Its own component rather than a third section of `MarketClinicalStatements`,
 * because an interaction is shaped differently: it names what it is *with*, and
 * that list is never empty.
 */
export function MarketInteractions({
  medicinalProductId,
}: MarketInteractionsProps) {
  const [recording, setRecording] = useState(false);
  const [editing, setEditing] = useState<{
    statementId: string;
    population?: Population;
  } | null>(null);

  const { data, error } = useInteractions(medicinalProductId);
  const removePopulation = useRemoveStatementPopulation();

  const interactions = data ?? [];

  return (
    <section className="space-y-3" data-testid="market-interactions">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h2 className="text-lg font-semibold">Interactions</h2>

        <Button
          size="sm"
          onClick={() => setRecording(true)}
          data-testid="record-interaction"
        >
          Record
        </Button>
      </div>

      {error && (
        <p className="text-sm text-destructive">Failed to load interactions.</p>
      )}

      {removePopulation.isError && (
        <p
          className="text-sm text-destructive"
          data-testid="remove-interaction-population-error"
        >
          {(removePopulation.error as Error).message}
        </p>
      )}

      {interactions.length === 0 && !error && (
        <div
          className="rounded-lg border border-dashed p-6 text-center"
          data-testid="interactions-empty"
        >
          <p className="text-sm text-muted-foreground">
            Nothing recorded here yet.
          </p>
        </div>
      )}

      <ul className="space-y-3">
        {interactions.map((interaction) => (
          <li
            key={interaction.id}
            className="rounded-lg border p-4"
            data-testid="interaction-row"
          >
            <div className="flex flex-wrap items-baseline gap-2">
              {interaction.interactants.map((interactant) => (
                <Badge
                  key={interactant.id}
                  variant={interactant.substanceId ? "default" : "secondary"}
                  data-testid="interactant"
                >
                  {interactant.description}
                  {/* Said out loud when the optional link is set, because a
                      linked interactant is findable from the substance and an
                      unlinked one is not. */}
                  {interactant.substanceName && " ↔"}
                </Badge>
              ))}

              <span className="text-xs text-muted-foreground">
                {interaction.interactionTypeDisplay}
              </span>

              {interaction.severityDisplay && (
                <Badge variant="outline" data-testid="interaction-severity">
                  {interaction.severityDisplay}
                </Badge>
              )}
            </div>

            <p className="mt-1 text-sm">{interaction.labelText}</p>

            {interaction.management && (
              <p
                className="mt-1 text-sm text-muted-foreground"
                data-testid="interaction-management"
              >
                {interaction.management}
              </p>
            )}

            <div className="mt-2 space-y-1">
              {interaction.populations.map((population) => (
                <div
                  key={population.id}
                  className="flex flex-wrap items-center gap-2 text-sm"
                  data-testid="interaction-population-row"
                >
                  <Badge variant="secondary">{describe(population)}</Badge>

                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-6 px-2 text-xs"
                    onClick={() =>
                      setEditing({
                        statementId: interaction.id,
                        population,
                      })
                    }
                    data-testid="correct-interaction-population"
                  >
                    Correct
                  </Button>

                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-6 px-2 text-xs"
                    onClick={() =>
                      removePopulation.mutate({
                        kind: "interactions",
                        statementId: interaction.id,
                        populationId: population.id,
                      })
                    }
                    data-testid="remove-interaction-population"
                  >
                    Remove
                  </Button>
                </div>
              ))}
            </div>

            <Button
              variant="outline"
              size="sm"
              className="mt-2"
              onClick={() => setEditing({ statementId: interaction.id })}
              data-testid="add-interaction-population"
            >
              Add population
            </Button>
          </li>
        ))}
      </ul>

      <RecordInteractionDialog
        medicinalProductId={medicinalProductId}
        open={recording}
        onOpenChange={setRecording}
      />

      {editing && (
        <PopulationDialog
          kind="interactions"
          statementId={editing.statementId}
          population={editing.population}
          open
          onOpenChange={(next) => !next && setEditing(null)}
        />
      )}
    </section>
  );
}
