import { useState } from "react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";

import { useContraindications } from "../hooks/useContraindications";
import { useRemoveStatementPopulation } from "../hooks/useRemoveStatementPopulation";
import { useUndesirableEffects } from "../hooks/useUndesirableEffects";
import type { Population } from "../types/Indication";
import type { StatementKind } from "../types/StatementKind";

import { PopulationDialog } from "./PopulationDialog";
import { RecordStatementDialog } from "./RecordStatementDialog";

interface MarketClinicalStatementsProps {
  medicinalProductId: string;
}

/** One row's worth of statement, whichever kind it is. */
interface StatementRow {
  id: string;
  heading: string;
  labelText: string;
  qualifier: string | null;
  populations: Population[];
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
 * What the approved label says beyond what the product is authorised for.
 *
 * **Neither section shows a history**, and that is the design: both are content
 * inside an approved label, so what changes them is a new `LocalLabel` revision
 * rather than a decision recorded here (ADR-059, EPIC-018 S004).
 */
export function MarketClinicalStatements({
  medicinalProductId,
}: MarketClinicalStatementsProps) {
  const [recording, setRecording] = useState<{
    kind: Exclude<StatementKind, "indications">;
    title: string;
  } | null>(null);

  const [editing, setEditing] = useState<{
    kind: StatementKind;
    statementId: string;
    population?: Population;
  } | null>(null);

  const contraindications = useContraindications(medicinalProductId);
  const effects = useUndesirableEffects(medicinalProductId);
  const removePopulation = useRemoveStatementPopulation();

  const sections: {
    kind: Exclude<StatementKind, "indications">;
    title: string;
    empty: string;
    rows: StatementRow[];
    isError: boolean;
  }[] = [
    {
      kind: "contraindications",
      title: "Contraindications",
      empty: "Nothing recorded here yet.",
      isError: !!contraindications.error,
      rows: (contraindications.data ?? []).map((x) => ({
        id: x.id,
        heading: x.conditionDisplay,
        labelText: x.labelText,
        qualifier: null,
        populations: x.populations,
      })),
    },
    {
      kind: "undesirable-effects",
      title: "Side effects",
      empty: "Nothing recorded here yet.",
      isError: !!effects.error,
      rows: (effects.data ?? []).map((x) => ({
        id: x.id,
        heading: x.effectDisplay,
        labelText: x.labelText,
        // The one thing the three statement types do not share.
        qualifier: x.frequencyDisplay,
        populations: x.populations,
      })),
    },
  ];

  return (
    <>
      {sections.map((section) => (
        <section
          key={section.kind}
          className="space-y-3"
          data-testid={`market-${section.kind}`}
        >
          <div className="flex flex-wrap items-center justify-between gap-2">
            <h2 className="text-lg font-semibold">{section.title}</h2>

            <Button
              size="sm"
              onClick={() =>
                setRecording({ kind: section.kind, title: section.title })
              }
              data-testid={`record-${section.kind}`}
            >
              Record
            </Button>
          </div>

          {section.isError && (
            <p className="text-sm text-destructive">
              Failed to load {section.title.toLowerCase()}.
            </p>
          )}

          {section.rows.length === 0 && !section.isError && (
            <div
              className="rounded-lg border border-dashed p-6 text-center"
              data-testid={`${section.kind}-empty`}
            >
              <p className="text-sm text-muted-foreground">{section.empty}</p>
            </div>
          )}

          <ul className="space-y-3">
            {section.rows.map((row) => (
              <li
                key={row.id}
                className="rounded-lg border p-4"
                data-testid={`${section.kind}-row`}
              >
                <div className="flex flex-wrap items-baseline gap-3">
                  <span className="font-semibold">{row.heading}</span>

                  {row.qualifier && (
                    <Badge variant="secondary" data-testid="statement-frequency">
                      {row.qualifier}
                    </Badge>
                  )}
                </div>

                <p className="mt-1 text-sm">{row.labelText}</p>

                <div className="mt-2 space-y-1">
                  {row.populations.map((population) => (
                    <div
                      key={population.id}
                      className="flex flex-wrap items-center gap-2 text-sm"
                      data-testid="statement-population-row"
                    >
                      <Badge variant="secondary">{describe(population)}</Badge>

                      <Button
                        variant="ghost"
                        size="sm"
                        className="h-6 px-2 text-xs"
                        onClick={() =>
                          setEditing({
                            kind: section.kind,
                            statementId: row.id,
                            population,
                          })
                        }
                        data-testid="correct-statement-population"
                      >
                        Correct
                      </Button>

                      <Button
                        variant="ghost"
                        size="sm"
                        className="h-6 px-2 text-xs"
                        onClick={() =>
                          removePopulation.mutate({
                            kind: section.kind,
                            statementId: row.id,
                            populationId: population.id,
                          })
                        }
                        data-testid="remove-statement-population"
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
                  onClick={() =>
                    setEditing({ kind: section.kind, statementId: row.id })
                  }
                  data-testid="add-statement-population"
                >
                  Add population
                </Button>
              </li>
            ))}
          </ul>
        </section>
      ))}

      {/* A failed removal and a stale list look identical otherwise — SC-106. */}
      {removePopulation.isError && (
        <p
          className="text-sm text-destructive"
          data-testid="remove-statement-population-error"
        >
          {(removePopulation.error as Error).message}
        </p>
      )}

      {recording && (
        <RecordStatementDialog
          kind={recording.kind}
          title={`Record ${recording.title.toLowerCase().replace(/s$/, "")}`}
          medicinalProductId={medicinalProductId}
          open
          onOpenChange={(next) => !next && setRecording(null)}
        />
      )}

      {editing && (
        <PopulationDialog
          kind={editing.kind}
          statementId={editing.statementId}
          population={editing.population}
          open
          onOpenChange={(next) => !next && setEditing(null)}
        />
      )}
    </>
  );
}
