import { useState } from "react";
import { useParams } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { Field, FieldLabel } from "@/components/ui/field";
import { Page } from "@/shared/components/Page";
import { PageHeader } from "@/shared/components/PageHeader";

import { studyKindLabel, useStudies } from "../../studies";
import { useApplicationStudies } from "../hooks/useApplicationStudies";
import { useCiteStudy } from "../hooks/useCiteStudy";
import { useStopCitingStudy } from "../hooks/useStopCitingStudy";

/**
 * *"Which studies support this filing?"* — the question that otherwise gets
 * answered by reading file names.
 *
 * A citation is a claim the **application** makes, so it is recorded here and
 * not on the study. Distinct from the study a *placement* reports: that says
 * which study one document is about, this says what the filing rests on, and an
 * application can cite a study it has not yet filed a document for.
 */
export function ApplicationStudiesPage() {
  const { applicationId } = useParams();

  const [selected, setSelected] = useState("");

  const cited = useApplicationStudies(applicationId!);
  const registry = useStudies();
  const cite = useCiteStudy(applicationId!);
  const stop = useStopCitingStudy(applicationId!);

  const rows = cited.data ?? [];

  // Only what is not already cited: offering a duplicate would invite a click
  // the server treats as a no-op, which reads as the button being broken.
  const citable = (registry.data ?? []).filter(
    (study) => !rows.some((row) => row.studyId === study.id),
  );

  async function submit(event: React.FormEvent) {
    event.preventDefault();

    const study = citable.find((s) => s.id === selected);

    if (!study) return;

    try {
      await cite.mutateAsync({ id: study.id, kind: study.kind });
    } catch {
      return;
    }

    setSelected("");
  }

  return (
    <Page>
      <PageHeader
        title="Studies"
        description="The studies this application is supported by."
      />

      {cited.isLoading && (
        <p className="text-muted-foreground">Loading studies...</p>
      )}

      {cited.error && (
        <p className="text-destructive">Failed to load studies.</p>
      )}

      {!cited.isLoading && !cited.error && rows.length === 0 && (
        <div
          className="rounded-lg border border-dashed p-8 text-center"
          data-testid="application-studies-empty"
        >
          <h3 className="text-lg font-semibold">
            This application cites no studies yet.
          </h3>
        </div>
      )}

      <ul className="space-y-3">
        {rows.map((study) => (
          <li
            key={study.studyId}
            className="flex flex-wrap items-center gap-3 rounded-lg border p-4"
            data-testid="cited-study"
          >
            <span className="font-mono text-sm font-semibold">
              {study.sponsorStudyIdentifier}
            </span>

            <span className="text-xs text-muted-foreground">
              {studyKindLabel(study.kind)}
            </span>

            <span className="flex-1 text-sm">{study.title}</span>

            <Button
              size="sm"
              variant="ghost"
              data-testid="stop-citing-study"
              aria-label={`Stop citing ${study.sponsorStudyIdentifier}`}
              onClick={() => stop.mutate(study.studyId)}
            >
              Remove
            </Button>
          </li>
        ))}
      </ul>

      {stop.isError && (
        <p className="text-sm text-destructive" data-testid="stop-citing-error">
          {(stop.error as Error).message}
        </p>
      )}

      <form onSubmit={submit} className="mt-6 flex items-end gap-2">
        <Field className="flex-1">
          <FieldLabel htmlFor="studyToCite">Cite a study</FieldLabel>

          <select
            id="studyToCite"
            value={selected}
            onChange={(event) => setSelected(event.target.value)}
            className="h-9 w-full rounded-md border bg-transparent px-3 text-sm"
          >
            <option value="">Select a study</option>
            {citable.map((study) => (
              <option key={study.id} value={study.id}>
                {study.sponsorStudyIdentifier} — {study.title} (
                {studyKindLabel(study.kind)})
              </option>
            ))}
          </select>
        </Field>

        <Button type="submit" disabled={selected === "" || cite.isPending}>
          {cite.isPending ? "Citing..." : "Cite"}
        </Button>
      </form>

      {cite.isError && (
        <p className="text-sm text-destructive" data-testid="cite-study-error">
          {(cite.error as Error).message}
        </p>
      )}
    </Page>
  );
}
